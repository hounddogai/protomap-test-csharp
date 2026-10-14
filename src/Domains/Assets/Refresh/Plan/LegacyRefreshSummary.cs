namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy refresh summary is in its life cycle.</summary>
public enum LegacyRefreshSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy refresh summary and its current status.</summary>
public sealed class LegacyRefreshSummary(string code, string label, bool archived, double ceiling, double minimum, string checksum, string externalRef, int createdAt, LegacyRefreshSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public string ExternalRef { get; set; } = externalRef;
    public int CreatedAt { get; set; } = createdAt;
    public LegacyRefreshSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy refresh summary in status new.</summary>
    public static LegacyRefreshSummary NewLegacyRefreshSummary(string code, string label)
    {
        return new LegacyRefreshSummary(code, label, false, 0.0, 0.0, "", "", 0, LegacyRefreshSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRefreshSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRefreshSummaryStatus next)
    {
        switch (Status)
        {
            case LegacyRefreshSummaryStatus.New:
                return next == LegacyRefreshSummaryStatus.Done;
            case LegacyRefreshSummaryStatus.Queued:
                switch (next)
                {
                    case LegacyRefreshSummaryStatus.Running:
                    case LegacyRefreshSummaryStatus.Failed:
                        return true;
                }
                return false;
            case LegacyRefreshSummaryStatus.Running:
                return next == LegacyRefreshSummaryStatus.Done || next == LegacyRefreshSummaryStatus.Failed;
            case LegacyRefreshSummaryStatus.Done:
                return next == LegacyRefreshSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyRefreshSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy refresh summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Minimum * Ceiling, 2);
    }

    public LegacyRefreshSummary Copy()
    {
        return new LegacyRefreshSummary(
            Code,
            Label,
            Archived,
            Ceiling,
            Minimum,
            Checksum,
            ExternalRef,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
