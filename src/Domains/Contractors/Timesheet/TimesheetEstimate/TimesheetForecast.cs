namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet forecast is in its life cycle.</summary>
public enum TimesheetForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A timesheet forecast as the timesheet team keeps it.</summary>
public sealed class TimesheetForecast(string code, string name, int capacity, double fte, int tenureMonths, int graceDays, string employeeId, double weight, int leaveDays, double cost, TimesheetForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Fte { get; set; } = fte;
    public int TenureMonths { get; set; } = tenureMonths;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public double Cost { get; set; } = cost;
    public TimesheetForecastStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet forecast in status proposed.</summary>
    public static TimesheetForecast NewTimesheetForecast(string code, string name)
    {
        return new TimesheetForecast(code, name, 0, 0.0, 0, 0, "", 0.0, 0, 0.0, TimesheetForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetForecastStatus next)
    {
        switch (Status)
        {
            case TimesheetForecastStatus.Proposed:
                switch (next)
                {
                    case TimesheetForecastStatus.Rejected:
                    case TimesheetForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case TimesheetForecastStatus.Approved:
                switch (next)
                {
                    case TimesheetForecastStatus.Rejected:
                        return true;
                }
                return false;
            case TimesheetForecastStatus.Rejected:
                return next == TimesheetForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TimesheetForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"timesheet forecast"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Capacity * (double)GraceDays, 2);
    }

    public TimesheetForecast Copy()
    {
        return new TimesheetForecast(
            Code,
            Name,
            Capacity,
            Fte,
            TenureMonths,
            GraceDays,
            EmployeeId,
            Weight,
            LeaveDays,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
