namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending desk entry is in its life cycle.</summary>
public enum PendingDeskEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending desk entry as the desk team keeps it.</summary>
public sealed class PendingDeskEntry(string code, string label, string department, double score, bool pinned, string unit, PendingDeskEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public PendingDeskEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pending desk entry in status idle.</summary>
    public static PendingDeskEntry NewPendingDeskEntry(string code, string label)
    {
        return new PendingDeskEntry(code, label, "", 0.0, false, "", PendingDeskEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeskEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeskEntryStatus next)
    {
        switch (Status)
        {
            case PendingDeskEntryStatus.Idle:
                return next == PendingDeskEntryStatus.Stopped;
            case PendingDeskEntryStatus.Warming:
                return next == PendingDeskEntryStatus.Ready;
            case PendingDeskEntryStatus.Ready:
                return next == PendingDeskEntryStatus.Draining || next == PendingDeskEntryStatus.Stopped;
            case PendingDeskEntryStatus.Draining:
                return next == PendingDeskEntryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingDeskEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending desk entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending desk entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PendingDeskEntry Copy()
    {
        return new PendingDeskEntry(Code, Label, Department, Score, Pinned, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
