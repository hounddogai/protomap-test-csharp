namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan forecast is in its life cycle.</summary>
public enum ReturnPlanForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A return plan forecast as the return plan team keeps it.</summary>
public sealed class ReturnPlanForecast(string code, string title, int attempts, int windowDays, double budget, double hours, string team, int durationMinutes, string employeeId, string name, ReturnPlanForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public double Hours { get; set; } = hours;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public ReturnPlanForecastStatus Status { get; set; } = status;

    /// <summary>Starts a return plan forecast in status created.</summary>
    public static ReturnPlanForecast NewReturnPlanForecast(string code, string title)
    {
        return new ReturnPlanForecast(code, title, 0, 0, 0.0, 0.0, "", 0, "", "", ReturnPlanForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanForecastStatus next)
    {
        switch (Status)
        {
            case ReturnPlanForecastStatus.Created:
                return next == ReturnPlanForecastStatus.Published;
            case ReturnPlanForecastStatus.Validated:
                return next == ReturnPlanForecastStatus.Published || next == ReturnPlanForecastStatus.Retired;
            case ReturnPlanForecastStatus.Published:
                switch (next)
                {
                    case ReturnPlanForecastStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"return plan forecast"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Budget - Hours, 2);
    }

    public ReturnPlanForecast Copy()
    {
        return new ReturnPlanForecast(
            Code,
            Title,
            Attempts,
            WindowDays,
            Budget,
            Hours,
            Team,
            DurationMinutes,
            EmployeeId,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
