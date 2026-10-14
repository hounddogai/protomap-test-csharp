namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet estimate is in its life cycle.</summary>
public enum TimesheetEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a timesheet estimate and its current status.</summary>
public sealed class TimesheetEstimate(string code, string label, double discount, int quantity, string name, int createdAt, double budget, string category, string department, bool approved, int startsAt, TimesheetEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public string Department { get; set; } = department;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public TimesheetEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet estimate in status planned.</summary>
    public static TimesheetEstimate NewTimesheetEstimate(string code, string label)
    {
        return new TimesheetEstimate(
            code,
            label,
            0.0,
            0,
            "",
            0,
            0.0,
            "",
            "",
            false,
            0,
            TimesheetEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetEstimateStatus next)
    {
        switch (Status)
        {
            case TimesheetEstimateStatus.Planned:
                return next == TimesheetEstimateStatus.Scheduled || next == TimesheetEstimateStatus.Completed;
            case TimesheetEstimateStatus.Scheduled:
                switch (next)
                {
                    case TimesheetEstimateStatus.Completed:
                    case TimesheetEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case TimesheetEstimateStatus.InProgress:
                return next == TimesheetEstimateStatus.Completed;
            case TimesheetEstimateStatus.Completed:
                return next == TimesheetEstimateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TimesheetEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt + (double)StartsAt, 2);
    }

    public TimesheetEstimate Copy()
    {
        return new TimesheetEstimate(
            Code,
            Label,
            Discount,
            Quantity,
            Name,
            CreatedAt,
            Budget,
            Category,
            Department,
            Approved,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
