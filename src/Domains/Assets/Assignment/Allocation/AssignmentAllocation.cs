namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment allocation is in its life cycle.</summary>
public enum AssignmentAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A assignment allocation. Its code is unique within a tenant.</summary>
public sealed class AssignmentAllocation(string code, string label, double minimum, int batchSize, string costCenter, bool active, int priority, int quantity, List<string> tags, string location, int updatedAt, AssignmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public bool Active { get; set; } = active;
    public int Priority { get; set; } = priority;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public AssignmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a assignment allocation in status draft.</summary>
    public static AssignmentAllocation NewAssignmentAllocation(string code, string label)
    {
        return new AssignmentAllocation(
            code,
            label,
            0.0,
            0,
            "",
            false,
            0,
            0,
            new List<string>(),
            "",
            0,
            AssignmentAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentAllocationStatus next)
    {
        switch (Status)
        {
            case AssignmentAllocationStatus.Draft:
                switch (next)
                {
                    case AssignmentAllocationStatus.Closed:
                        return true;
                }
                return false;
            case AssignmentAllocationStatus.Pending:
                switch (next)
                {
                    case AssignmentAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentAllocationStatus.Active:
                switch (next)
                {
                    case AssignmentAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentAllocationStatus.Suspended:
                return next == AssignmentAllocationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssignmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"assignment allocation"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity + (double)UpdatedAt, 2);
    }

    public AssignmentAllocation Copy()
    {
        return new AssignmentAllocation(
            Code,
            Label,
            Minimum,
            BatchSize,
            CostCenter,
            Active,
            Priority,
            Quantity,
            Tags,
            Location,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
