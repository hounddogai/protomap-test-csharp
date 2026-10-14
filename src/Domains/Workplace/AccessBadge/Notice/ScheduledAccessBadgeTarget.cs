namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled access badge target is in its life cycle.</summary>
public enum ScheduledAccessBadgeTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled access badge target. Its code is unique within a tenant.</summary>
public sealed class ScheduledAccessBadgeTarget(string code, string name, int tenureMonths, string ownerId, double threshold, bool approved, ScheduledAccessBadgeTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string OwnerId { get; set; } = ownerId;
    public double Threshold { get; set; } = threshold;
    public bool Approved { get; set; } = approved;
    public ScheduledAccessBadgeTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled access badge target in status requested.</summary>
    public static ScheduledAccessBadgeTarget NewScheduledAccessBadgeTarget(string code, string name)
    {
        return new ScheduledAccessBadgeTarget(
            code,
            name,
            0,
            "",
            0.0,
            false,
            ScheduledAccessBadgeTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccessBadgeTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccessBadgeTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledAccessBadgeTargetStatus.Requested:
                switch (next)
                {
                    case ScheduledAccessBadgeTargetStatus.Granted:
                    case ScheduledAccessBadgeTargetStatus.Revoked:
                        return true;
                }
                return false;
            case ScheduledAccessBadgeTargetStatus.Granted:
                return next == ScheduledAccessBadgeTargetStatus.Revoked || next == ScheduledAccessBadgeTargetStatus.Expired;
            case ScheduledAccessBadgeTargetStatus.Revoked:
                return next == ScheduledAccessBadgeTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledAccessBadgeTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled access badge target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Threshold - (double)TenureMonths, 2);
    }

    public ScheduledAccessBadgeTarget Copy()
    {
        return new ScheduledAccessBadgeTarget(Code, Name, TenureMonths, OwnerId, Threshold, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
