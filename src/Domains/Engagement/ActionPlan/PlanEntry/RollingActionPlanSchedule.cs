namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling action plan schedule is in its life cycle.</summary>
public enum RollingActionPlanScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling action plan schedule as the action plan team keeps it.</summary>
public sealed class RollingActionPlanSchedule(string code, string name, string employeeId, double discount, string location, string currency, string skill, string source, RollingActionPlanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Source { get; set; } = source;
    public RollingActionPlanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling action plan schedule in status planned.</summary>
    public static RollingActionPlanSchedule NewRollingActionPlanSchedule(string code, string name)
    {
        return new RollingActionPlanSchedule(
            code,
            name,
            "",
            0.0,
            "",
            "",
            "",
            "",
            RollingActionPlanScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingActionPlanScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingActionPlanScheduleStatus next)
    {
        switch (Status)
        {
            case RollingActionPlanScheduleStatus.Planned:
                return next == RollingActionPlanScheduleStatus.Cancelled;
            case RollingActionPlanScheduleStatus.Scheduled:
                switch (next)
                {
                    case RollingActionPlanScheduleStatus.Completed:
                        return true;
                }
                return false;
            case RollingActionPlanScheduleStatus.InProgress:
                switch (next)
                {
                    case RollingActionPlanScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingActionPlanScheduleStatus.Completed:
                return next == RollingActionPlanScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingActionPlanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling action plan schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling action plan schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public RollingActionPlanSchedule Copy()
    {
        return new RollingActionPlanSchedule(
            Code,
            Name,
            EmployeeId,
            Discount,
            Location,
            Currency,
            Skill,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
