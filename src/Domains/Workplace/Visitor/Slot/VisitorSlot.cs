namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor slot is in its life cycle.</summary>
public enum VisitorSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A visitor slot as the visitor team keeps it.</summary>
public sealed class VisitorSlot(string code, string label, int retries, double threshold, int leaveDays, string timezone, string notes, VisitorSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public string Notes { get; set; } = notes;
    public VisitorSlotStatus Status { get; set; } = status;

    /// <summary>Starts a visitor slot in status new.</summary>
    public static VisitorSlot NewVisitorSlot(string code, string label)
    {
        return new VisitorSlot(code, label, 0, 0.0, 0, "", "", VisitorSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorSlotStatus next)
    {
        switch (Status)
        {
            case VisitorSlotStatus.New:
                return next == VisitorSlotStatus.Running;
            case VisitorSlotStatus.Queued:
                return next == VisitorSlotStatus.Running || next == VisitorSlotStatus.Done;
            case VisitorSlotStatus.Running:
                return next == VisitorSlotStatus.Failed;
            case VisitorSlotStatus.Done:
                return next == VisitorSlotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)LeaveDays - (double)Retries, 2);
    }

    public VisitorSlot Copy()
    {
        return new VisitorSlot(Code, Label, Retries, Threshold, LeaveDays, Timezone, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
