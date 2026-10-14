namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped rate milestone is in its life cycle.</summary>
public enum CappedRateMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped rate milestone, with what the planners need to act on it.</summary>
public sealed class CappedRateMilestone(string code, string label, bool approved, int tenureMonths, string location, int limit, int durationMinutes, string category, int graceDays, CappedRateMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public CappedRateMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a capped rate milestone in status draft.</summary>
    public static CappedRateMilestone NewCappedRateMilestone(string code, string label)
    {
        return new CappedRateMilestone(code, label, false, 0, "", 0, 0, "", 0, CappedRateMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRateMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRateMilestoneStatus next)
    {
        switch (Status)
        {
            case CappedRateMilestoneStatus.Draft:
                switch (next)
                {
                    case CappedRateMilestoneStatus.Pending:
                    case CappedRateMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case CappedRateMilestoneStatus.Pending:
                return next == CappedRateMilestoneStatus.Suspended || next == CappedRateMilestoneStatus.Closed;
            case CappedRateMilestoneStatus.Active:
                switch (next)
                {
                    case CappedRateMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case CappedRateMilestoneStatus.Suspended:
                switch (next)
                {
                    case CappedRateMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRateMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped rate milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped rate milestone"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)GraceDays + (double)DurationMinutes, 2);
    }

    public CappedRateMilestone Copy()
    {
        return new CappedRateMilestone(
            Code,
            Label,
            Approved,
            TenureMonths,
            Location,
            Limit,
            DurationMinutes,
            Category,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
