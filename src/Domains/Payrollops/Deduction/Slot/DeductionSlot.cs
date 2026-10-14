namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction slot is in its life cycle.</summary>
public enum DeductionSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A deduction slot as the deduction team keeps it.</summary>
public sealed class DeductionSlot(string code, string label, double ceiling, string title, string checksum, string unit, int priority, int startsAt, int endsAt, string costCenter, string name, DeductionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public DeductionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a deduction slot in status created.</summary>
    public static DeductionSlot NewDeductionSlot(string code, string label)
    {
        return new DeductionSlot(code, label, 0.0, "", "", "", 0, 0, 0, "", "", DeductionSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionSlotStatus next)
    {
        switch (Status)
        {
            case DeductionSlotStatus.Created:
                switch (next)
                {
                    case DeductionSlotStatus.Published:
                    case DeductionSlotStatus.Retired:
                        return true;
                }
                return false;
            case DeductionSlotStatus.Validated:
                return next == DeductionSlotStatus.Published || next == DeductionSlotStatus.Retired;
            case DeductionSlotStatus.Published:
                return next == DeductionSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeductionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling + (double)EndsAt, 2);
    }

    public DeductionSlot Copy()
    {
        return new DeductionSlot(
            Code,
            Label,
            Ceiling,
            Title,
            Checksum,
            Unit,
            Priority,
            StartsAt,
            EndsAt,
            CostCenter,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
