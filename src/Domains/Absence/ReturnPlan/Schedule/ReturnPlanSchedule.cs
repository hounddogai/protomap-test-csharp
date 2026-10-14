namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan schedule is in its life cycle.</summary>
public enum ReturnPlanScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One return plan schedule, with what the planners need to act on it.</summary>
public sealed class ReturnPlanSchedule(string code, string name, int quantity, string channel, string externalRef, int revision, string costCenter, ReturnPlanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public string ExternalRef { get; set; } = externalRef;
    public int Revision { get; set; } = revision;
    public string CostCenter { get; set; } = costCenter;
    public ReturnPlanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a return plan schedule in status new.</summary>
    public static ReturnPlanSchedule NewReturnPlanSchedule(string code, string name)
    {
        return new ReturnPlanSchedule(code, name, 0, "", "", 0, "", ReturnPlanScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanScheduleStatus next)
    {
        switch (Status)
        {
            case ReturnPlanScheduleStatus.New:
                return next == ReturnPlanScheduleStatus.Done;
            case ReturnPlanScheduleStatus.Queued:
                switch (next)
                {
                    case ReturnPlanScheduleStatus.Done:
                    case ReturnPlanScheduleStatus.Failed:
                        return true;
                }
                return false;
            case ReturnPlanScheduleStatus.Running:
                switch (next)
                {
                    case ReturnPlanScheduleStatus.Failed:
                        return true;
                }
                return false;
            case ReturnPlanScheduleStatus.Done:
                return next == ReturnPlanScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ReturnPlanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"return plan schedule"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Quantity - (double)Revision, 2);
    }

    public ReturnPlanSchedule Copy()
    {
        return new ReturnPlanSchedule(Code, Name, Quantity, Channel, ExternalRef, Revision, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
