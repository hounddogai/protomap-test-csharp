namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global vendor slot is in its life cycle.</summary>
public enum GlobalVendorSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global vendor slot and its current status.</summary>
public sealed class GlobalVendorSlot(string code, string label, bool approved, string grade, int limit, string department, GlobalVendorSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Grade { get; set; } = grade;
    public int Limit { get; set; } = limit;
    public string Department { get; set; } = department;
    public GlobalVendorSlotStatus Status { get; set; } = status;

    /// <summary>Starts a global vendor slot in status idle.</summary>
    public static GlobalVendorSlot NewGlobalVendorSlot(string code, string label)
    {
        return new GlobalVendorSlot(code, label, false, "", 0, "", GlobalVendorSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalVendorSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalVendorSlotStatus next)
    {
        switch (Status)
        {
            case GlobalVendorSlotStatus.Idle:
                switch (next)
                {
                    case GlobalVendorSlotStatus.Warming:
                    case GlobalVendorSlotStatus.Ready:
                        return true;
                }
                return false;
            case GlobalVendorSlotStatus.Warming:
                return next == GlobalVendorSlotStatus.Stopped;
            case GlobalVendorSlotStatus.Ready:
                return next == GlobalVendorSlotStatus.Draining;
            case GlobalVendorSlotStatus.Draining:
                return next == GlobalVendorSlotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalVendorSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global vendor slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public GlobalVendorSlot Copy()
    {
        return new GlobalVendorSlot(Code, Label, Approved, Grade, Limit, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
