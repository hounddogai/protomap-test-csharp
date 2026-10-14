namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge assignment is in its life cycle.</summary>
public enum AccessBadgeAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A access badge assignment as the access badge team keeps it.</summary>
public sealed class AccessBadgeAssignment(string code, string name, int capacity, string summary, string phone, string reviewer, string jobCode, int windowDays, AccessBadgeAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public AccessBadgeAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a access badge assignment in status open.</summary>
    public static AccessBadgeAssignment NewAccessBadgeAssignment(string code, string name)
    {
        return new AccessBadgeAssignment(code, name, 0, "", "", "", "", 0, AccessBadgeAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeAssignmentStatus next)
    {
        switch (Status)
        {
            case AccessBadgeAssignmentStatus.Open:
                switch (next)
                {
                    case AccessBadgeAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case AccessBadgeAssignmentStatus.InReview:
                return next == AccessBadgeAssignmentStatus.Resolved || next == AccessBadgeAssignmentStatus.Reopened;
            case AccessBadgeAssignmentStatus.Resolved:
                switch (next)
                {
                    case AccessBadgeAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case AccessBadgeAssignmentStatus.Reopened:
                return next == AccessBadgeAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccessBadgeAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays + (double)Capacity, 2);
    }

    public AccessBadgeAssignment Copy()
    {
        return new AccessBadgeAssignment(
            Code,
            Name,
            Capacity,
            Summary,
            Phone,
            Reviewer,
            JobCode,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
