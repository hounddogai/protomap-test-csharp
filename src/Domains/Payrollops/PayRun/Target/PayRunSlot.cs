namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run slot is in its life cycle.</summary>
public enum PayRunSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pay run slot. Its code is unique within a tenant.</summary>
public sealed class PayRunSlot(string code, string label, int version, string category, double hours, PayRunSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public PayRunSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pay run slot in status new.</summary>
    public static PayRunSlot NewPayRunSlot(string code, string label)
    {
        return new PayRunSlot(code, label, 0, "", 0.0, PayRunSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunSlotStatus next)
    {
        switch (Status)
        {
            case PayRunSlotStatus.New:
                return next == PayRunSlotStatus.Done || next == PayRunSlotStatus.Failed;
            case PayRunSlotStatus.Queued:
                return next == PayRunSlotStatus.Running || next == PayRunSlotStatus.Failed;
            case PayRunSlotStatus.Running:
                return next == PayRunSlotStatus.Done || next == PayRunSlotStatus.Failed;
            case PayRunSlotStatus.Done:
                return next == PayRunSlotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PayRunSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours * (double)Version, 2);
    }

    public PayRunSlot Copy()
    {
        return new PayRunSlot(Code, Label, Version, Category, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
