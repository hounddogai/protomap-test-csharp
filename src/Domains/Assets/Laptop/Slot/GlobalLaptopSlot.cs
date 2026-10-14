namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global laptop slot is in its life cycle.</summary>
public enum GlobalLaptopSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a global laptop slot and its current status.</summary>
public sealed class GlobalLaptopSlot(string code, string name, string ownerId, int tenureMonths, bool active, string costCenter, double amount, GlobalLaptopSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public double Amount { get; set; } = amount;
    public GlobalLaptopSlotStatus Status { get; set; } = status;

    /// <summary>Starts a global laptop slot in status created.</summary>
    public static GlobalLaptopSlot NewGlobalLaptopSlot(string code, string name)
    {
        return new GlobalLaptopSlot(code, name, "", 0, false, "", 0.0, GlobalLaptopSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLaptopSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLaptopSlotStatus next)
    {
        switch (Status)
        {
            case GlobalLaptopSlotStatus.Created:
                return next == GlobalLaptopSlotStatus.Published || next == GlobalLaptopSlotStatus.Retired;
            case GlobalLaptopSlotStatus.Validated:
                switch (next)
                {
                    case GlobalLaptopSlotStatus.Published:
                    case GlobalLaptopSlotStatus.Retired:
                        return true;
                }
                return false;
            case GlobalLaptopSlotStatus.Published:
                switch (next)
                {
                    case GlobalLaptopSlotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalLaptopSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global laptop slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global laptop slot"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Amount - (double)TenureMonths, 2);
    }

    public GlobalLaptopSlot Copy()
    {
        return new GlobalLaptopSlot(Code, Name, OwnerId, TenureMonths, Active, CostCenter, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
