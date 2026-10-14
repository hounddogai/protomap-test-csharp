namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty slot is in its life cycle.</summary>
public enum WarrantySlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A warranty slot as the warranty team keeps it.</summary>
public sealed class WarrantySlot(string code, string label, bool archived, string externalRef, string location, int durationMinutes, int limit, WarrantySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Limit { get; set; } = limit;
    public WarrantySlotStatus Status { get; set; } = status;

    /// <summary>Starts a warranty slot in status proposed.</summary>
    public static WarrantySlot NewWarrantySlot(string code, string label)
    {
        return new WarrantySlot(code, label, false, "", "", 0, 0, WarrantySlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantySlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantySlotStatus next)
    {
        switch (Status)
        {
            case WarrantySlotStatus.Proposed:
                return next == WarrantySlotStatus.Approved;
            case WarrantySlotStatus.Approved:
                switch (next)
                {
                    case WarrantySlotStatus.Rejected:
                    case WarrantySlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case WarrantySlotStatus.Rejected:
                switch (next)
                {
                    case WarrantySlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"warranty slot"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Limit + (double)DurationMinutes, 2);
    }

    public WarrantySlot Copy()
    {
        return new WarrantySlot(Code, Label, Archived, ExternalRef, Location, DurationMinutes, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
