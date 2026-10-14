namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor schedule is in its life cycle.</summary>
public enum LenientVisitorScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient visitor schedule as the visitor team keeps it.</summary>
public sealed class LenientVisitorSchedule(string code, string label, int graceDays, double maximum, int limit, double budget, LenientVisitorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Maximum { get; set; } = maximum;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public LenientVisitorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor schedule in status draft.</summary>
    public static LenientVisitorSchedule NewLenientVisitorSchedule(string code, string label)
    {
        return new LenientVisitorSchedule(code, label, 0, 0.0, 0, 0.0, LenientVisitorScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorScheduleStatus next)
    {
        switch (Status)
        {
            case LenientVisitorScheduleStatus.Draft:
                return next == LenientVisitorScheduleStatus.Suspended;
            case LenientVisitorScheduleStatus.Pending:
                switch (next)
                {
                    case LenientVisitorScheduleStatus.Closed:
                        return true;
                }
                return false;
            case LenientVisitorScheduleStatus.Active:
                return next == LenientVisitorScheduleStatus.Suspended || next == LenientVisitorScheduleStatus.Closed;
            case LenientVisitorScheduleStatus.Suspended:
                return next == LenientVisitorScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientVisitorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Budget * (double)GraceDays, 2);
    }

    public LenientVisitorSchedule Copy()
    {
        return new LenientVisitorSchedule(Code, Label, GraceDays, Maximum, Limit, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
