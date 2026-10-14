namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled adjustment allocation is in its life cycle.</summary>
public enum ScheduledAdjustmentAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled adjustment allocation, with what the planners need to act on it.</summary>
public sealed class ScheduledAdjustmentAllocation(string code, string title, string badge, double weight, Dictionary<string, string> attributes, string timezone, string region, string managerId, string location, bool visible, ScheduledAdjustmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Timezone { get; set; } = timezone;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public string Location { get; set; } = location;
    public bool Visible { get; set; } = visible;
    public ScheduledAdjustmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled adjustment allocation in status new.</summary>
    public static ScheduledAdjustmentAllocation NewScheduledAdjustmentAllocation(string code, string title)
    {
        return new ScheduledAdjustmentAllocation(
            code,
            title,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            false,
            ScheduledAdjustmentAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAdjustmentAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAdjustmentAllocationStatus next)
    {
        switch (Status)
        {
            case ScheduledAdjustmentAllocationStatus.New:
                switch (next)
                {
                    case ScheduledAdjustmentAllocationStatus.Queued:
                    case ScheduledAdjustmentAllocationStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledAdjustmentAllocationStatus.Queued:
                switch (next)
                {
                    case ScheduledAdjustmentAllocationStatus.Running:
                    case ScheduledAdjustmentAllocationStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledAdjustmentAllocationStatus.Running:
                return next == ScheduledAdjustmentAllocationStatus.Done || next == ScheduledAdjustmentAllocationStatus.Failed;
            case ScheduledAdjustmentAllocationStatus.Done:
                switch (next)
                {
                    case ScheduledAdjustmentAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAdjustmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled adjustment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled adjustment allocation"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public ScheduledAdjustmentAllocation Copy()
    {
        return new ScheduledAdjustmentAllocation(
            Code,
            Title,
            Badge,
            Weight,
            Attributes,
            Timezone,
            Region,
            ManagerId,
            Location,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
