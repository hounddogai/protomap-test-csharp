namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient timesheet plan is in its life cycle.</summary>
public enum LenientTimesheetPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient timesheet plan. Its code is unique within a tenant.</summary>
public sealed class LenientTimesheetPlan(string code, string name, string segment, string location, double maximum, string managerId, List<string> tags, LenientTimesheetPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public List<string> Tags { get; set; } = tags;
    public LenientTimesheetPlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient timesheet plan in status new.</summary>
    public static LenientTimesheetPlan NewLenientTimesheetPlan(string code, string name)
    {
        return new LenientTimesheetPlan(
            code,
            name,
            "",
            "",
            0.0,
            "",
            new List<string>(),
            LenientTimesheetPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTimesheetPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTimesheetPlanStatus next)
    {
        switch (Status)
        {
            case LenientTimesheetPlanStatus.New:
                switch (next)
                {
                    case LenientTimesheetPlanStatus.Running:
                    case LenientTimesheetPlanStatus.Failed:
                        return true;
                }
                return false;
            case LenientTimesheetPlanStatus.Queued:
                return next == LenientTimesheetPlanStatus.Done || next == LenientTimesheetPlanStatus.Failed;
            case LenientTimesheetPlanStatus.Running:
                return next == LenientTimesheetPlanStatus.Failed;
            case LenientTimesheetPlanStatus.Done:
                switch (next)
                {
                    case LenientTimesheetPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTimesheetPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient timesheet plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient timesheet plan"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public LenientTimesheetPlan Copy()
    {
        return new LenientTimesheetPlan(Code, Name, Segment, Location, Maximum, ManagerId, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
