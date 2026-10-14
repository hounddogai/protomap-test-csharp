namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled engagement target is in its life cycle.</summary>
public enum ScheduledEngagementTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled engagement target as the engagement team keeps it.</summary>
public sealed class ScheduledEngagementTarget(string code, string name, string label, double weight, string costCenter, double ceiling, double fte, int batchSize, string category, double cost, ScheduledEngagementTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string CostCenter { get; set; } = costCenter;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public double Cost { get; set; } = cost;
    public ScheduledEngagementTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled engagement target in status planned.</summary>
    public static ScheduledEngagementTarget NewScheduledEngagementTarget(string code, string name)
    {
        return new ScheduledEngagementTarget(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0,
            "",
            0.0,
            ScheduledEngagementTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEngagementTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEngagementTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledEngagementTargetStatus.Planned:
                return next == ScheduledEngagementTargetStatus.Scheduled || next == ScheduledEngagementTargetStatus.Completed;
            case ScheduledEngagementTargetStatus.Scheduled:
                switch (next)
                {
                    case ScheduledEngagementTargetStatus.InProgress:
                    case ScheduledEngagementTargetStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledEngagementTargetStatus.InProgress:
                switch (next)
                {
                    case ScheduledEngagementTargetStatus.Completed:
                    case ScheduledEngagementTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledEngagementTargetStatus.Completed:
                return next == ScheduledEngagementTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledEngagementTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled engagement target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Weight + Fte, 2);
    }

    public ScheduledEngagementTarget Copy()
    {
        return new ScheduledEngagementTarget(
            Code,
            Name,
            Label,
            Weight,
            CostCenter,
            Ceiling,
            Fte,
            BatchSize,
            Category,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
