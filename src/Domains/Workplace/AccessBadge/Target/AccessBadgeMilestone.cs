namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge milestone is in its life cycle.</summary>
public enum AccessBadgeMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A access badge milestone as the access badge team keeps it.</summary>
public sealed class AccessBadgeMilestone(string code, string name, double rate, string skill, string unit, string title, int batchSize, string currency, AccessBadgeMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public AccessBadgeMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a access badge milestone in status open.</summary>
    public static AccessBadgeMilestone NewAccessBadgeMilestone(string code, string name)
    {
        return new AccessBadgeMilestone(code, name, 0.0, "", "", "", 0, "", AccessBadgeMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeMilestoneStatus next)
    {
        switch (Status)
        {
            case AccessBadgeMilestoneStatus.Open:
                switch (next)
                {
                    case AccessBadgeMilestoneStatus.Resolved:
                    case AccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeMilestoneStatus.InReview:
                switch (next)
                {
                    case AccessBadgeMilestoneStatus.Resolved:
                    case AccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeMilestoneStatus.Resolved:
                return next == AccessBadgeMilestoneStatus.Reopened || next == AccessBadgeMilestoneStatus.Archived;
            case AccessBadgeMilestoneStatus.Reopened:
                switch (next)
                {
                    case AccessBadgeMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)BatchSize * Rate, 2);
    }

    public AccessBadgeMilestone Copy()
    {
        return new AccessBadgeMilestone(Code, Name, Rate, Skill, Unit, Title, BatchSize, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
