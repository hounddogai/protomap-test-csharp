namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy visitor milestone is in its life cycle.</summary>
public enum LegacyVisitorMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy visitor milestone and its current status.</summary>
public sealed class LegacyVisitorMilestone(string code, string name, string summary, bool active, string unit, string employeeId, int endsAt, string badge, string team, double threshold, string category, LegacyVisitorMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public bool Active { get; set; } = active;
    public string Unit { get; set; } = unit;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public double Threshold { get; set; } = threshold;
    public string Category { get; set; } = category;
    public LegacyVisitorMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a legacy visitor milestone in status new.</summary>
    public static LegacyVisitorMilestone NewLegacyVisitorMilestone(string code, string name)
    {
        return new LegacyVisitorMilestone(
            code,
            name,
            "",
            false,
            "",
            "",
            0,
            "",
            "",
            0.0,
            "",
            LegacyVisitorMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyVisitorMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyVisitorMilestoneStatus next)
    {
        switch (Status)
        {
            case LegacyVisitorMilestoneStatus.New:
                return next == LegacyVisitorMilestoneStatus.Running;
            case LegacyVisitorMilestoneStatus.Queued:
                return next == LegacyVisitorMilestoneStatus.Running || next == LegacyVisitorMilestoneStatus.Failed;
            case LegacyVisitorMilestoneStatus.Running:
                switch (next)
                {
                    case LegacyVisitorMilestoneStatus.Done:
                    case LegacyVisitorMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case LegacyVisitorMilestoneStatus.Done:
                return next == LegacyVisitorMilestoneStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyVisitorMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy visitor milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Threshold + (double)EndsAt, 2);
    }

    public LegacyVisitorMilestone Copy()
    {
        return new LegacyVisitorMilestone(
            Code,
            Name,
            Summary,
            Active,
            Unit,
            EmployeeId,
            EndsAt,
            Badge,
            Team,
            Threshold,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
