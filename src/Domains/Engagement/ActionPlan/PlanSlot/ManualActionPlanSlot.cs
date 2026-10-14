namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual action plan slot is in its life cycle.</summary>
public enum ManualActionPlanSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One manual action plan slot, with what the planners need to act on it.</summary>
public sealed class ManualActionPlanSlot(string code, string name, string jobCode, string locale, int graceDays, string segment, string description, double ratio, ManualActionPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Locale { get; set; } = locale;
    public int GraceDays { get; set; } = graceDays;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public double Ratio { get; set; } = ratio;
    public ManualActionPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a manual action plan slot in status new.</summary>
    public static ManualActionPlanSlot NewManualActionPlanSlot(string code, string name)
    {
        return new ManualActionPlanSlot(code, name, "", "", 0, "", "", 0.0, ManualActionPlanSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualActionPlanSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualActionPlanSlotStatus next)
    {
        switch (Status)
        {
            case ManualActionPlanSlotStatus.New:
                switch (next)
                {
                    case ManualActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
            case ManualActionPlanSlotStatus.Queued:
                switch (next)
                {
                    case ManualActionPlanSlotStatus.Done:
                    case ManualActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
            case ManualActionPlanSlotStatus.Running:
                switch (next)
                {
                    case ManualActionPlanSlotStatus.Done:
                        return true;
                }
                return false;
            case ManualActionPlanSlotStatus.Done:
                switch (next)
                {
                    case ManualActionPlanSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualActionPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual action plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual action plan slot"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio * (double)GraceDays, 2);
    }

    public ManualActionPlanSlot Copy()
    {
        return new ManualActionPlanSlot(Code, Name, JobCode, Locale, GraceDays, Segment, Description, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
