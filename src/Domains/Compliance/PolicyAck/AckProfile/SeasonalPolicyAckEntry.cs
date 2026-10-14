namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal policy ack entry is in its life cycle.</summary>
public enum SeasonalPolicyAckEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One seasonal policy ack entry, with what the planners need to act on it.</summary>
public sealed class SeasonalPolicyAckEntry(string code, string title, bool approved, double cost, int tenureMonths, string summary, double score, double hours, string category, double fte, string location, SeasonalPolicyAckEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double Cost { get; set; } = cost;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public double Hours { get; set; } = hours;
    public string Category { get; set; } = category;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public SeasonalPolicyAckEntryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal policy ack entry in status new.</summary>
    public static SeasonalPolicyAckEntry NewSeasonalPolicyAckEntry(string code, string title)
    {
        return new SeasonalPolicyAckEntry(
            code,
            title,
            false,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            0.0,
            "",
            SeasonalPolicyAckEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPolicyAckEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPolicyAckEntryStatus next)
    {
        switch (Status)
        {
            case SeasonalPolicyAckEntryStatus.New:
                return next == SeasonalPolicyAckEntryStatus.Queued;
            case SeasonalPolicyAckEntryStatus.Queued:
                switch (next)
                {
                    case SeasonalPolicyAckEntryStatus.Running:
                    case SeasonalPolicyAckEntryStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalPolicyAckEntryStatus.Running:
                return next == SeasonalPolicyAckEntryStatus.Done;
            case SeasonalPolicyAckEntryStatus.Done:
                return next == SeasonalPolicyAckEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalPolicyAckEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal policy ack entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Cost + Fte, 2);
    }

    public SeasonalPolicyAckEntry Copy()
    {
        return new SeasonalPolicyAckEntry(
            Code,
            Title,
            Approved,
            Cost,
            TenureMonths,
            Summary,
            Score,
            Hours,
            Category,
            Fte,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
