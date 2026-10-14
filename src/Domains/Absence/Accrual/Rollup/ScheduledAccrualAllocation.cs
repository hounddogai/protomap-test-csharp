namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled accrual allocation is in its life cycle.</summary>
public enum ScheduledAccrualAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scheduled accrual allocation and its current status.</summary>
public sealed class ScheduledAccrualAllocation(string code, string name, string managerId, double threshold, double minimum, string locale, string jobCode, double amount, bool approved, string currency, int capacity, ScheduledAccrualAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public int Capacity { get; set; } = capacity;
    public ScheduledAccrualAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled accrual allocation in status planned.</summary>
    public static ScheduledAccrualAllocation NewScheduledAccrualAllocation(string code, string name)
    {
        return new ScheduledAccrualAllocation(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            "",
            0.0,
            false,
            "",
            0,
            ScheduledAccrualAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccrualAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccrualAllocationStatus next)
    {
        switch (Status)
        {
            case ScheduledAccrualAllocationStatus.Planned:
                return next == ScheduledAccrualAllocationStatus.Completed;
            case ScheduledAccrualAllocationStatus.Scheduled:
                return next == ScheduledAccrualAllocationStatus.InProgress || next == ScheduledAccrualAllocationStatus.Cancelled;
            case ScheduledAccrualAllocationStatus.InProgress:
                switch (next)
                {
                    case ScheduledAccrualAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledAccrualAllocationStatus.Completed:
                return next == ScheduledAccrualAllocationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledAccrualAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled accrual allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled accrual allocation"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount + Threshold, 2);
    }

    public ScheduledAccrualAllocation Copy()
    {
        return new ScheduledAccrualAllocation(
            Code,
            Name,
            ManagerId,
            Threshold,
            Minimum,
            Locale,
            JobCode,
            Amount,
            Approved,
            Currency,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
