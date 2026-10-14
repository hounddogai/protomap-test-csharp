namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan slot is in its life cycle.</summary>
public enum FloorPlanSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One floor plan slot, with what the planners need to act on it.</summary>
public sealed class FloorPlanSlot(string code, string label, string employeeId, int endsAt, string currency, double score, double budget, string grade, FloorPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public string Currency { get; set; } = currency;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public string Grade { get; set; } = grade;
    public FloorPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan slot in status created.</summary>
    public static FloorPlanSlot NewFloorPlanSlot(string code, string label)
    {
        return new FloorPlanSlot(code, label, "", 0, "", 0.0, 0.0, "", FloorPlanSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanSlotStatus next)
    {
        switch (Status)
        {
            case FloorPlanSlotStatus.Created:
                return next == FloorPlanSlotStatus.Validated || next == FloorPlanSlotStatus.Retired;
            case FloorPlanSlotStatus.Validated:
                return next == FloorPlanSlotStatus.Retired;
            case FloorPlanSlotStatus.Published:
                return next == FloorPlanSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FloorPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - Budget, 2);
    }

    public FloorPlanSlot Copy()
    {
        return new FloorPlanSlot(Code, Label, EmployeeId, EndsAt, Currency, Score, Budget, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
