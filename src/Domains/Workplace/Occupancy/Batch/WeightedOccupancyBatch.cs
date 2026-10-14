namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted occupancy batch is in its life cycle.</summary>
public enum WeightedOccupancyBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One weighted occupancy batch, with what the planners need to act on it.</summary>
public sealed class WeightedOccupancyBatch(string code, string name, double ceiling, double discount, double floorValue, double ratio, int priority, int updatedAt, string timezone, string category, WeightedOccupancyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public double FloorValue { get; set; } = floorValue;
    public double Ratio { get; set; } = ratio;
    public int Priority { get; set; } = priority;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Timezone { get; set; } = timezone;
    public string Category { get; set; } = category;
    public WeightedOccupancyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a weighted occupancy batch in status planned.</summary>
    public static WeightedOccupancyBatch NewWeightedOccupancyBatch(string code, string name)
    {
        return new WeightedOccupancyBatch(
            code,
            name,
            0.0,
            0.0,
            0.0,
            0.0,
            0,
            0,
            "",
            "",
            WeightedOccupancyBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedOccupancyBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedOccupancyBatchStatus next)
    {
        switch (Status)
        {
            case WeightedOccupancyBatchStatus.Planned:
                return next == WeightedOccupancyBatchStatus.InProgress;
            case WeightedOccupancyBatchStatus.Scheduled:
                switch (next)
                {
                    case WeightedOccupancyBatchStatus.InProgress:
                    case WeightedOccupancyBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedOccupancyBatchStatus.InProgress:
                switch (next)
                {
                    case WeightedOccupancyBatchStatus.Completed:
                    case WeightedOccupancyBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedOccupancyBatchStatus.Completed:
                return next == WeightedOccupancyBatchStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedOccupancyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted occupancy batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round((double)UpdatedAt + (double)Priority, 2);
    }

    public WeightedOccupancyBatch Copy()
    {
        return new WeightedOccupancyBatch(
            Code,
            Name,
            Ceiling,
            Discount,
            FloorValue,
            Ratio,
            Priority,
            UpdatedAt,
            Timezone,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
