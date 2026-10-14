namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan slot is in its life cycle.</summary>
public enum ReturnPlanSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a return plan slot and its current status.</summary>
public sealed class ReturnPlanSlot(string code, string label, string description, bool archived, string unit, double weight, int updatedAt, bool active, bool pinned, double hours, ReturnPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public double Weight { get; set; } = weight;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Active { get; set; } = active;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public ReturnPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a return plan slot in status created.</summary>
    public static ReturnPlanSlot NewReturnPlanSlot(string code, string label)
    {
        return new ReturnPlanSlot(
            code,
            label,
            "",
            false,
            "",
            0.0,
            0,
            false,
            false,
            0.0,
            ReturnPlanSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanSlotStatus next)
    {
        switch (Status)
        {
            case ReturnPlanSlotStatus.Created:
                switch (next)
                {
                    case ReturnPlanSlotStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanSlotStatus.Validated:
                switch (next)
                {
                    case ReturnPlanSlotStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanSlotStatus.Published:
                return next == ReturnPlanSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"return plan slot"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt + Hours, 2);
    }

    public ReturnPlanSlot Copy()
    {
        return new ReturnPlanSlot(
            Code,
            Label,
            Description,
            Archived,
            Unit,
            Weight,
            UpdatedAt,
            Active,
            Pinned,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
