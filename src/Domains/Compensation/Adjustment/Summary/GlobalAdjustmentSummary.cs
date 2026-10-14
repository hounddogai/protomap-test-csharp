namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global adjustment summary is in its life cycle.</summary>
public enum GlobalAdjustmentSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global adjustment summary, with what the planners need to act on it.</summary>
public sealed class GlobalAdjustmentSummary(string code, string title, string summary, bool approved, string location, int attempts, int endsAt, string region, string channel, GlobalAdjustmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public string Channel { get; set; } = channel;
    public GlobalAdjustmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a global adjustment summary in status requested.</summary>
    public static GlobalAdjustmentSummary NewGlobalAdjustmentSummary(string code, string title)
    {
        return new GlobalAdjustmentSummary(
            code,
            title,
            "",
            false,
            "",
            0,
            0,
            "",
            "",
            GlobalAdjustmentSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAdjustmentSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAdjustmentSummaryStatus next)
    {
        switch (Status)
        {
            case GlobalAdjustmentSummaryStatus.Requested:
                switch (next)
                {
                    case GlobalAdjustmentSummaryStatus.Expired:
                        return true;
                }
                return false;
            case GlobalAdjustmentSummaryStatus.Granted:
                return next == GlobalAdjustmentSummaryStatus.Revoked;
            case GlobalAdjustmentSummaryStatus.Revoked:
                return next == GlobalAdjustmentSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalAdjustmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global adjustment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts + (double)EndsAt, 2);
    }

    public GlobalAdjustmentSummary Copy()
    {
        return new GlobalAdjustmentSummary(
            Code,
            Title,
            Summary,
            Approved,
            Location,
            Attempts,
            EndsAt,
            Region,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
