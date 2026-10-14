namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding cycle is in its life cycle.</summary>
public enum OffboardingCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding cycle. Its code is unique within a tenant.</summary>
public sealed class OffboardingCycle(string code, string name, double amount, int attempts, string costCenter, int limit, double threshold, string department, double minimum, double hours, OffboardingCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public string CostCenter { get; set; } = costCenter;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public string Department { get; set; } = department;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public OffboardingCycleStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding cycle in status planned.</summary>
    public static OffboardingCycle NewOffboardingCycle(string code, string name)
    {
        return new OffboardingCycle(code, name, 0.0, 0, "", 0, 0.0, "", 0.0, 0.0, OffboardingCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingCycleStatus next)
    {
        switch (Status)
        {
            case OffboardingCycleStatus.Planned:
                return next == OffboardingCycleStatus.Scheduled || next == OffboardingCycleStatus.InProgress;
            case OffboardingCycleStatus.Scheduled:
                switch (next)
                {
                    case OffboardingCycleStatus.InProgress:
                        return true;
                }
                return false;
            case OffboardingCycleStatus.InProgress:
                return next == OffboardingCycleStatus.Completed || next == OffboardingCycleStatus.Cancelled;
            case OffboardingCycleStatus.Completed:
                switch (next)
                {
                    case OffboardingCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Hours * Minimum, 2);
    }

    public OffboardingCycle Copy()
    {
        return new OffboardingCycle(
            Code,
            Name,
            Amount,
            Attempts,
            CostCenter,
            Limit,
            Threshold,
            Department,
            Minimum,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
