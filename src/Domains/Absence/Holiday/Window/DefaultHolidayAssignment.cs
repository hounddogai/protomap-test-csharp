namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default holiday assignment is in its life cycle.</summary>
public enum DefaultHolidayAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default holiday assignment, with what the planners need to act on it.</summary>
public sealed class DefaultHolidayAssignment(string code, string title, string label, double budget, double ceiling, int retries, int quantity, string unit, int limit, DefaultHolidayAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public int Quantity { get; set; } = quantity;
    public string Unit { get; set; } = unit;
    public int Limit { get; set; } = limit;
    public DefaultHolidayAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a default holiday assignment in status new.</summary>
    public static DefaultHolidayAssignment NewDefaultHolidayAssignment(string code, string title)
    {
        return new DefaultHolidayAssignment(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            0,
            "",
            0,
            DefaultHolidayAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHolidayAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHolidayAssignmentStatus next)
    {
        switch (Status)
        {
            case DefaultHolidayAssignmentStatus.New:
                switch (next)
                {
                    case DefaultHolidayAssignmentStatus.Queued:
                    case DefaultHolidayAssignmentStatus.Running:
                        return true;
                }
                return false;
            case DefaultHolidayAssignmentStatus.Queued:
                switch (next)
                {
                    case DefaultHolidayAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case DefaultHolidayAssignmentStatus.Running:
                switch (next)
                {
                    case DefaultHolidayAssignmentStatus.Done:
                    case DefaultHolidayAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case DefaultHolidayAssignmentStatus.Done:
                switch (next)
                {
                    case DefaultHolidayAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultHolidayAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default holiday assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default holiday assignment"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries + (double)Quantity, 2);
    }

    public DefaultHolidayAssignment Copy()
    {
        return new DefaultHolidayAssignment(
            Code,
            Title,
            Label,
            Budget,
            Ceiling,
            Retries,
            Quantity,
            Unit,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
