namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered leave request override is in its life cycle.</summary>
public enum TieredLeaveRequestOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered leave request override as the leave request team keeps it.</summary>
public sealed class TieredLeaveRequestOverride(string code, string title, string employeeId, string team, double minimum, double threshold, double fte, string category, string country, string reviewer, TieredLeaveRequestOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Team { get; set; } = team;
    public double Minimum { get; set; } = minimum;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public string Category { get; set; } = category;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public TieredLeaveRequestOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a tiered leave request override in status proposed.</summary>
    public static TieredLeaveRequestOverride NewTieredLeaveRequestOverride(string code, string title)
    {
        return new TieredLeaveRequestOverride(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            "",
            TieredLeaveRequestOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLeaveRequestOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLeaveRequestOverrideStatus next)
    {
        switch (Status)
        {
            case TieredLeaveRequestOverrideStatus.Proposed:
                return next == TieredLeaveRequestOverrideStatus.Rejected || next == TieredLeaveRequestOverrideStatus.Withdrawn;
            case TieredLeaveRequestOverrideStatus.Approved:
                return next == TieredLeaveRequestOverrideStatus.Withdrawn;
            case TieredLeaveRequestOverrideStatus.Rejected:
                switch (next)
                {
                    case TieredLeaveRequestOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLeaveRequestOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered leave request override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered leave request override"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + Minimum, 2);
    }

    public TieredLeaveRequestOverride Copy()
    {
        return new TieredLeaveRequestOverride(
            Code,
            Title,
            EmployeeId,
            Team,
            Minimum,
            Threshold,
            Fte,
            Category,
            Country,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
