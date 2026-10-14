namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched floor plan snapshot is in its life cycle.</summary>
public enum BatchedFloorPlanSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched floor plan snapshot. Its code is unique within a tenant.</summary>
public sealed class BatchedFloorPlanSnapshot(string code, string name, double weight, double floorValue, double threshold, Dictionary<string, string> attributes, int batchSize, string contactEmail, string employeeId, int graceDays, string country, BatchedFloorPlanSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public double Threshold { get; set; } = threshold;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public BatchedFloorPlanSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched floor plan snapshot in status proposed.</summary>
    public static BatchedFloorPlanSnapshot NewBatchedFloorPlanSnapshot(string code, string name)
    {
        return new BatchedFloorPlanSnapshot(
            code,
            name,
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            0,
            "",
            BatchedFloorPlanSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFloorPlanSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFloorPlanSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedFloorPlanSnapshotStatus.Proposed:
                return next == BatchedFloorPlanSnapshotStatus.Rejected || next == BatchedFloorPlanSnapshotStatus.Withdrawn;
            case BatchedFloorPlanSnapshotStatus.Approved:
                switch (next)
                {
                    case BatchedFloorPlanSnapshotStatus.Rejected:
                    case BatchedFloorPlanSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedFloorPlanSnapshotStatus.Rejected:
                switch (next)
                {
                    case BatchedFloorPlanSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedFloorPlanSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched floor plan snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched floor plan snapshot"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedValue()
    {
        return Math.Round(Weight - Threshold, 2);
    }

    public BatchedFloorPlanSnapshot Copy()
    {
        return new BatchedFloorPlanSnapshot(
            Code,
            Name,
            Weight,
            FloorValue,
            Threshold,
            Attributes,
            BatchSize,
            ContactEmail,
            EmployeeId,
            GraceDays,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
