namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient accrual slot is in its life cycle.</summary>
public enum LenientAccrualSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient accrual slot as the accrual team keeps it.</summary>
public sealed class LenientAccrualSlot(string code, string label, int batchSize, string contactEmail, int windowDays, double threshold, int version, LenientAccrualSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public int Version { get; set; } = version;
    public LenientAccrualSlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient accrual slot in status proposed.</summary>
    public static LenientAccrualSlot NewLenientAccrualSlot(string code, string label)
    {
        return new LenientAccrualSlot(code, label, 0, "", 0, 0.0, 0, LenientAccrualSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAccrualSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAccrualSlotStatus next)
    {
        switch (Status)
        {
            case LenientAccrualSlotStatus.Proposed:
                return next == LenientAccrualSlotStatus.Approved || next == LenientAccrualSlotStatus.Withdrawn;
            case LenientAccrualSlotStatus.Approved:
                switch (next)
                {
                    case LenientAccrualSlotStatus.Rejected:
                    case LenientAccrualSlotStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientAccrualSlotStatus.Rejected:
                return next == LenientAccrualSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientAccrualSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient accrual slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)WindowDays - (double)BatchSize, 2);
    }

    public LenientAccrualSlot Copy()
    {
        return new LenientAccrualSlot(Code, Label, BatchSize, ContactEmail, WindowDays, Threshold, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
