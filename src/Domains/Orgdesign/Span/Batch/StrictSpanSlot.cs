namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict span slot is in its life cycle.</summary>
public enum StrictSpanSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict span slot, with what the planners need to act on it.</summary>
public sealed class StrictSpanSlot(string code, string label, int leaveDays, string unit, double amount, StrictSpanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public StrictSpanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a strict span slot in status idle.</summary>
    public static StrictSpanSlot NewStrictSpanSlot(string code, string label)
    {
        return new StrictSpanSlot(code, label, 0, "", 0.0, StrictSpanSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSpanSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSpanSlotStatus next)
    {
        switch (Status)
        {
            case StrictSpanSlotStatus.Idle:
                return next == StrictSpanSlotStatus.Stopped;
            case StrictSpanSlotStatus.Warming:
                switch (next)
                {
                    case StrictSpanSlotStatus.Ready:
                        return true;
                }
                return false;
            case StrictSpanSlotStatus.Ready:
                return next == StrictSpanSlotStatus.Draining || next == StrictSpanSlotStatus.Stopped;
            case StrictSpanSlotStatus.Draining:
                return next == StrictSpanSlotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictSpanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict span slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount + (double)LeaveDays, 2);
    }

    public StrictSpanSlot Copy()
    {
        return new StrictSpanSlot(Code, Label, LeaveDays, Unit, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
