namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict accrual forecast is in its life cycle.</summary>
public enum StrictAccrualForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict accrual forecast as the accrual team keeps it.</summary>
public sealed class StrictAccrualForecast(string code, string name, string label, string reviewer, string managerId, int priority, string contactEmail, string phone, string source, StrictAccrualForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public string Phone { get; set; } = phone;
    public string Source { get; set; } = source;
    public StrictAccrualForecastStatus Status { get; set; } = status;

    /// <summary>Starts a strict accrual forecast in status proposed.</summary>
    public static StrictAccrualForecast NewStrictAccrualForecast(string code, string name)
    {
        return new StrictAccrualForecast(
            code,
            name,
            "",
            "",
            "",
            0,
            "",
            "",
            "",
            StrictAccrualForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccrualForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccrualForecastStatus next)
    {
        switch (Status)
        {
            case StrictAccrualForecastStatus.Proposed:
                return next == StrictAccrualForecastStatus.Approved || next == StrictAccrualForecastStatus.Withdrawn;
            case StrictAccrualForecastStatus.Approved:
                switch (next)
                {
                    case StrictAccrualForecastStatus.Rejected:
                    case StrictAccrualForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictAccrualForecastStatus.Rejected:
                return next == StrictAccrualForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictAccrualForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict accrual forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict accrual forecast"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public StrictAccrualForecast Copy()
    {
        return new StrictAccrualForecast(
            Code,
            Name,
            Label,
            Reviewer,
            ManagerId,
            Priority,
            ContactEmail,
            Phone,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
