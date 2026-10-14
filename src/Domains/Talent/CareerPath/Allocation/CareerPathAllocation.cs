namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path allocation is in its life cycle.</summary>
public enum CareerPathAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One career path allocation, with what the planners need to act on it.</summary>
public sealed class CareerPathAllocation(string code, string label, string timezone, int limit, double weight, int capacity, int quantity, int durationMinutes, string channel, string checksum, CareerPathAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public CareerPathAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a career path allocation in status planned.</summary>
    public static CareerPathAllocation NewCareerPathAllocation(string code, string label)
    {
        return new CareerPathAllocation(
            code,
            label,
            "",
            0,
            0.0,
            0,
            0,
            0,
            "",
            "",
            CareerPathAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathAllocationStatus next)
    {
        switch (Status)
        {
            case CareerPathAllocationStatus.Planned:
                return next == CareerPathAllocationStatus.Cancelled;
            case CareerPathAllocationStatus.Scheduled:
                switch (next)
                {
                    case CareerPathAllocationStatus.InProgress:
                        return true;
                }
                return false;
            case CareerPathAllocationStatus.InProgress:
                switch (next)
                {
                    case CareerPathAllocationStatus.Completed:
                    case CareerPathAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathAllocationStatus.Completed:
                return next == CareerPathAllocationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path allocation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit * Weight, 2);
    }

    public CareerPathAllocation Copy()
    {
        return new CareerPathAllocation(
            Code,
            Label,
            Timezone,
            Limit,
            Weight,
            Capacity,
            Quantity,
            DurationMinutes,
            Channel,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
