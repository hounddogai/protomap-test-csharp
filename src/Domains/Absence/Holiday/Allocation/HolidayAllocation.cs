namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday allocation is in its life cycle.</summary>
public enum HolidayAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One holiday allocation, with what the planners need to act on it.</summary>
public sealed class HolidayAllocation(string code, string name, int sequence, int graceDays, double floorValue, double margin, double score, string currency, int retries, HolidayAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public int GraceDays { get; set; } = graceDays;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public int Retries { get; set; } = retries;
    public HolidayAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a holiday allocation in status planned.</summary>
    public static HolidayAllocation NewHolidayAllocation(string code, string name)
    {
        return new HolidayAllocation(code, name, 0, 0, 0.0, 0.0, 0.0, "", 0, HolidayAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayAllocationStatus next)
    {
        switch (Status)
        {
            case HolidayAllocationStatus.Planned:
                switch (next)
                {
                    case HolidayAllocationStatus.Scheduled:
                    case HolidayAllocationStatus.InProgress:
                        return true;
                }
                return false;
            case HolidayAllocationStatus.Scheduled:
                return next == HolidayAllocationStatus.Completed;
            case HolidayAllocationStatus.InProgress:
                return next == HolidayAllocationStatus.Completed || next == HolidayAllocationStatus.Cancelled;
            case HolidayAllocationStatus.Completed:
                switch (next)
                {
                    case HolidayAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday allocation"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)GraceDays + (double)Sequence, 2);
    }

    public HolidayAllocation Copy()
    {
        return new HolidayAllocation(
            Code,
            Name,
            Sequence,
            GraceDays,
            FloorValue,
            Margin,
            Score,
            Currency,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
