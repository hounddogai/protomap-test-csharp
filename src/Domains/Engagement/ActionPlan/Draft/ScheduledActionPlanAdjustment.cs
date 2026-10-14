namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled action plan adjustment is in its life cycle.</summary>
public enum ScheduledActionPlanAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled action plan adjustment as the action plan team keeps it.</summary>
public sealed class ScheduledActionPlanAdjustment(string code, string name, double cost, int durationMinutes, string phone, ScheduledActionPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Phone { get; set; } = phone;
    public ScheduledActionPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled action plan adjustment in status planned.</summary>
    public static ScheduledActionPlanAdjustment NewScheduledActionPlanAdjustment(string code, string name)
    {
        return new ScheduledActionPlanAdjustment(
            code,
            name,
            0.0,
            0,
            "",
            ScheduledActionPlanAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledActionPlanAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledActionPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case ScheduledActionPlanAdjustmentStatus.Planned:
                switch (next)
                {
                    case ScheduledActionPlanAdjustmentStatus.Scheduled:
                        return true;
                }
                return false;
            case ScheduledActionPlanAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case ScheduledActionPlanAdjustmentStatus.InProgress:
                    case ScheduledActionPlanAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledActionPlanAdjustmentStatus.InProgress:
                switch (next)
                {
                    case ScheduledActionPlanAdjustmentStatus.Completed:
                    case ScheduledActionPlanAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledActionPlanAdjustmentStatus.Completed:
                return next == ScheduledActionPlanAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledActionPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled action plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes + Cost, 2);
    }

    public ScheduledActionPlanAdjustment Copy()
    {
        return new ScheduledActionPlanAdjustment(Code, Name, Cost, DurationMinutes, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
