namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay band allocation is in its life cycle.</summary>
public enum TieredPayBandAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tiered pay band allocation, with what the planners need to act on it.</summary>
public sealed class TieredPayBandAllocation(string code, string name, int priority, List<string> tags, string description, int sequence, bool approved, string reviewer, string segment, int revision, double hours, TieredPayBandAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public bool Approved { get; set; } = approved;
    public string Reviewer { get; set; } = reviewer;
    public string Segment { get; set; } = segment;
    public int Revision { get; set; } = revision;
    public double Hours { get; set; } = hours;
    public TieredPayBandAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay band allocation in status open.</summary>
    public static TieredPayBandAllocation NewTieredPayBandAllocation(string code, string name)
    {
        return new TieredPayBandAllocation(
            code,
            name,
            0,
            new List<string>(),
            "",
            0,
            false,
            "",
            "",
            0,
            0.0,
            TieredPayBandAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayBandAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayBandAllocationStatus next)
    {
        switch (Status)
        {
            case TieredPayBandAllocationStatus.Open:
                return next == TieredPayBandAllocationStatus.Resolved;
            case TieredPayBandAllocationStatus.InReview:
                switch (next)
                {
                    case TieredPayBandAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case TieredPayBandAllocationStatus.Resolved:
                switch (next)
                {
                    case TieredPayBandAllocationStatus.Reopened:
                    case TieredPayBandAllocationStatus.Archived:
                        return true;
                }
                return false;
            case TieredPayBandAllocationStatus.Reopened:
                switch (next)
                {
                    case TieredPayBandAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPayBandAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay band allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Sequence + (double)Priority, 2);
    }

    public TieredPayBandAllocation Copy()
    {
        return new TieredPayBandAllocation(
            Code,
            Name,
            Priority,
            Tags,
            Description,
            Sequence,
            Approved,
            Reviewer,
            Segment,
            Revision,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
