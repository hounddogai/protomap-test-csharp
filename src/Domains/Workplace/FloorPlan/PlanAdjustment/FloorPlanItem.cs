namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan item is in its life cycle.</summary>
public enum FloorPlanItemStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A floor plan item. Its code is unique within a tenant.</summary>
public sealed class FloorPlanItem(string code, string name, string location, string externalRef, bool approved, double amount, string phone, string description, int sequence, FloorPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public FloorPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan item in status proposed.</summary>
    public static FloorPlanItem NewFloorPlanItem(string code, string name)
    {
        return new FloorPlanItem(code, name, "", "", false, 0.0, "", "", 0, FloorPlanItemStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanItemStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanItemStatus next)
    {
        switch (Status)
        {
            case FloorPlanItemStatus.Proposed:
                switch (next)
                {
                    case FloorPlanItemStatus.Approved:
                    case FloorPlanItemStatus.Rejected:
                        return true;
                }
                return false;
            case FloorPlanItemStatus.Approved:
                return next == FloorPlanItemStatus.Withdrawn;
            case FloorPlanItemStatus.Rejected:
                switch (next)
                {
                    case FloorPlanItemStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Sequence * Amount, 2);
    }

    public FloorPlanItem Copy()
    {
        return new FloorPlanItem(
            Code,
            Name,
            Location,
            ExternalRef,
            Approved,
            Amount,
            Phone,
            Description,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
