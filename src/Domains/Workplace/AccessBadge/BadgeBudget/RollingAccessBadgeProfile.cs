namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling access badge profile is in its life cycle.</summary>
public enum RollingAccessBadgeProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling access badge profile and its current status.</summary>
public sealed class RollingAccessBadgeProfile(string code, string label, int windowDays, string employeeId, int createdAt, int capacity, string badge, string phone, int revision, RollingAccessBadgeProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public RollingAccessBadgeProfileStatus Status { get; set; } = status;

    /// <summary>Starts a rolling access badge profile in status open.</summary>
    public static RollingAccessBadgeProfile NewRollingAccessBadgeProfile(string code, string label)
    {
        return new RollingAccessBadgeProfile(
            code,
            label,
            0,
            "",
            0,
            0,
            "",
            "",
            0,
            RollingAccessBadgeProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccessBadgeProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccessBadgeProfileStatus next)
    {
        switch (Status)
        {
            case RollingAccessBadgeProfileStatus.Open:
                return next == RollingAccessBadgeProfileStatus.InReview || next == RollingAccessBadgeProfileStatus.Resolved;
            case RollingAccessBadgeProfileStatus.InReview:
                switch (next)
                {
                    case RollingAccessBadgeProfileStatus.Archived:
                        return true;
                }
                return false;
            case RollingAccessBadgeProfileStatus.Resolved:
                switch (next)
                {
                    case RollingAccessBadgeProfileStatus.Reopened:
                        return true;
                }
                return false;
            case RollingAccessBadgeProfileStatus.Reopened:
                switch (next)
                {
                    case RollingAccessBadgeProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccessBadgeProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling access badge profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity + (double)WindowDays, 2);
    }

    public RollingAccessBadgeProfile Copy()
    {
        return new RollingAccessBadgeProfile(
            Code,
            Label,
            WindowDays,
            EmployeeId,
            CreatedAt,
            Capacity,
            Badge,
            Phone,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
