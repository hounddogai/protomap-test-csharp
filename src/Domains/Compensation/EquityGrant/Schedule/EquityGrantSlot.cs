namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant slot is in its life cycle.</summary>
public enum EquityGrantSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A equity grant slot. Its code is unique within a tenant.</summary>
public sealed class EquityGrantSlot(string code, string label, int createdAt, double floorValue, int retries, string locale, EquityGrantSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public string Locale { get; set; } = locale;
    public EquityGrantSlotStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant slot in status proposed.</summary>
    public static EquityGrantSlot NewEquityGrantSlot(string code, string label)
    {
        return new EquityGrantSlot(code, label, 0, 0.0, 0, "", EquityGrantSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantSlotStatus next)
    {
        switch (Status)
        {
            case EquityGrantSlotStatus.Proposed:
                switch (next)
                {
                    case EquityGrantSlotStatus.Rejected:
                    case EquityGrantSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case EquityGrantSlotStatus.Approved:
                switch (next)
                {
                    case EquityGrantSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case EquityGrantSlotStatus.Rejected:
                return next == EquityGrantSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(EquityGrantSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue + (double)Retries, 2);
    }

    public EquityGrantSlot Copy()
    {
        return new EquityGrantSlot(Code, Label, CreatedAt, FloorValue, Retries, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
