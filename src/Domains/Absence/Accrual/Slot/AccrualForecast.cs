namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual forecast is in its life cycle.</summary>
public enum AccrualForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A accrual forecast. Its code is unique within a tenant.</summary>
public sealed class AccrualForecast(string code, string name, string jobCode, double margin, string grade, double amount, double floorValue, string salaryBand, AccrualForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public AccrualForecastStatus Status { get; set; } = status;

    /// <summary>Starts a accrual forecast in status planned.</summary>
    public static AccrualForecast NewAccrualForecast(string code, string name)
    {
        return new AccrualForecast(code, name, "", 0.0, "", 0.0, 0.0, "", AccrualForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualForecastStatus next)
    {
        switch (Status)
        {
            case AccrualForecastStatus.Planned:
                switch (next)
                {
                    case AccrualForecastStatus.InProgress:
                    case AccrualForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualForecastStatus.Scheduled:
                switch (next)
                {
                    case AccrualForecastStatus.Completed:
                    case AccrualForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualForecastStatus.InProgress:
                return next == AccrualForecastStatus.Cancelled;
            case AccrualForecastStatus.Completed:
                switch (next)
                {
                    case AccrualForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue * Amount, 2);
    }

    public AccrualForecast Copy()
    {
        return new AccrualForecast(Code, Name, JobCode, Margin, Grade, Amount, FloorValue, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
