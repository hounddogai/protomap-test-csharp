namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall summary is in its life cycle.</summary>
public enum TownHallSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a town hall summary and its current status.</summary>
public sealed class TownHallSummary(string code, string label, int expiresAt, int graceDays, string currency, TownHallSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public string Currency { get; set; } = currency;
    public TownHallSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a town hall summary in status new.</summary>
    public static TownHallSummary NewTownHallSummary(string code, string label)
    {
        return new TownHallSummary(code, label, 0, 0, "", TownHallSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallSummaryStatus next)
    {
        switch (Status)
        {
            case TownHallSummaryStatus.New:
                switch (next)
                {
                    case TownHallSummaryStatus.Queued:
                        return true;
                }
                return false;
            case TownHallSummaryStatus.Queued:
                return next == TownHallSummaryStatus.Done || next == TownHallSummaryStatus.Failed;
            case TownHallSummaryStatus.Running:
                switch (next)
                {
                    case TownHallSummaryStatus.Done:
                    case TownHallSummaryStatus.Failed:
                        return true;
                }
                return false;
            case TownHallSummaryStatus.Done:
                return next == TownHallSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TownHallSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt * (double)GraceDays, 2);
    }

    public TownHallSummary Copy()
    {
        return new TownHallSummary(Code, Label, ExpiresAt, GraceDays, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
