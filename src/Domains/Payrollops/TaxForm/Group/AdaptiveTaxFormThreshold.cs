namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive tax form threshold is in its life cycle.</summary>
public enum AdaptiveTaxFormThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a adaptive tax form threshold and its current status.</summary>
public sealed class AdaptiveTaxFormThreshold(string code, string name, double ceiling, string salaryBand, int attempts, string employeeId, AdaptiveTaxFormThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public AdaptiveTaxFormThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive tax form threshold in status planned.</summary>
    public static AdaptiveTaxFormThreshold NewAdaptiveTaxFormThreshold(string code, string name)
    {
        return new AdaptiveTaxFormThreshold(code, name, 0.0, "", 0, "", AdaptiveTaxFormThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTaxFormThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTaxFormThresholdStatus next)
    {
        switch (Status)
        {
            case AdaptiveTaxFormThresholdStatus.Planned:
                switch (next)
                {
                    case AdaptiveTaxFormThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveTaxFormThresholdStatus.Scheduled:
                switch (next)
                {
                    case AdaptiveTaxFormThresholdStatus.InProgress:
                    case AdaptiveTaxFormThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveTaxFormThresholdStatus.InProgress:
                return next == AdaptiveTaxFormThresholdStatus.Cancelled;
            case AdaptiveTaxFormThresholdStatus.Completed:
                return next == AdaptiveTaxFormThresholdStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveTaxFormThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive tax form threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts - Ceiling, 2);
    }

    public AdaptiveTaxFormThreshold Copy()
    {
        return new AdaptiveTaxFormThreshold(Code, Name, Ceiling, SalaryBand, Attempts, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
