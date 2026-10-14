namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered occupancy record is in its life cycle.</summary>
public enum TieredOccupancyRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a tiered occupancy record and its current status.</summary>
public sealed class TieredOccupancyRecord(string code, string label, double cost, string segment, int capacity, string grade, int priority, TieredOccupancyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public TieredOccupancyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a tiered occupancy record in status planned.</summary>
    public static TieredOccupancyRecord NewTieredOccupancyRecord(string code, string label)
    {
        return new TieredOccupancyRecord(code, label, 0.0, "", 0, "", 0, TieredOccupancyRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredOccupancyRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredOccupancyRecordStatus next)
    {
        switch (Status)
        {
            case TieredOccupancyRecordStatus.Planned:
                return next == TieredOccupancyRecordStatus.InProgress || next == TieredOccupancyRecordStatus.Cancelled;
            case TieredOccupancyRecordStatus.Scheduled:
                switch (next)
                {
                    case TieredOccupancyRecordStatus.InProgress:
                        return true;
                }
                return false;
            case TieredOccupancyRecordStatus.InProgress:
                return next == TieredOccupancyRecordStatus.Completed || next == TieredOccupancyRecordStatus.Cancelled;
            case TieredOccupancyRecordStatus.Completed:
                return next == TieredOccupancyRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredOccupancyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered occupancy record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tiered occupancy record"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public TieredOccupancyRecord Copy()
    {
        return new TieredOccupancyRecord(Code, Label, Cost, Segment, Capacity, Grade, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
