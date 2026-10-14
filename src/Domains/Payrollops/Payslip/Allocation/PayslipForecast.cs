namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip forecast is in its life cycle.</summary>
public enum PayslipForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a payslip forecast and its current status.</summary>
public sealed class PayslipForecast(string code, string label, double margin, string jobCode, int windowDays, int retries, int capacity, string name, PayslipForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public PayslipForecastStatus Status { get; set; } = status;

    /// <summary>Starts a payslip forecast in status proposed.</summary>
    public static PayslipForecast NewPayslipForecast(string code, string label)
    {
        return new PayslipForecast(code, label, 0.0, "", 0, 0, 0, "", PayslipForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipForecastStatus next)
    {
        switch (Status)
        {
            case PayslipForecastStatus.Proposed:
                switch (next)
                {
                    case PayslipForecastStatus.Rejected:
                    case PayslipForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayslipForecastStatus.Approved:
                return next == PayslipForecastStatus.Withdrawn;
            case PayslipForecastStatus.Rejected:
                return next == PayslipForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayslipForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round((double)WindowDays - (double)Retries, 2);
    }

    public PayslipForecast Copy()
    {
        return new PayslipForecast(Code, Label, Margin, JobCode, WindowDays, Retries, Capacity, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
