namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy reorg summary is in its life cycle.</summary>
public enum LegacyReorgSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a legacy reorg summary and its current status.</summary>
public sealed class LegacyReorgSummary(string code, string title, string badge, double amount, bool archived, string channel, LegacyReorgSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public LegacyReorgSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy reorg summary in status planned.</summary>
    public static LegacyReorgSummary NewLegacyReorgSummary(string code, string title)
    {
        return new LegacyReorgSummary(code, title, "", 0.0, false, "", LegacyReorgSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyReorgSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyReorgSummaryStatus next)
    {
        switch (Status)
        {
            case LegacyReorgSummaryStatus.Planned:
                switch (next)
                {
                    case LegacyReorgSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyReorgSummaryStatus.Scheduled:
                switch (next)
                {
                    case LegacyReorgSummaryStatus.InProgress:
                        return true;
                }
                return false;
            case LegacyReorgSummaryStatus.InProgress:
                return next == LegacyReorgSummaryStatus.Completed;
            case LegacyReorgSummaryStatus.Completed:
                return next == LegacyReorgSummaryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyReorgSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy reorg summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LegacyReorgSummary Copy()
    {
        return new LegacyReorgSummary(Code, Title, Badge, Amount, Archived, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
