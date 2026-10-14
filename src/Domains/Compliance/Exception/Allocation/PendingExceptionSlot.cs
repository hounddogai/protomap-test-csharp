namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception slot is in its life cycle.</summary>
public enum PendingExceptionSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending exception slot. Its code is unique within a tenant.</summary>
public sealed class PendingExceptionSlot(string code, string title, int batchSize, int version, bool visible, int limit, string team, double amount, PendingExceptionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public PendingExceptionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception slot in status proposed.</summary>
    public static PendingExceptionSlot NewPendingExceptionSlot(string code, string title)
    {
        return new PendingExceptionSlot(code, title, 0, 0, false, 0, "", 0.0, PendingExceptionSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionSlotStatus next)
    {
        switch (Status)
        {
            case PendingExceptionSlotStatus.Proposed:
                return next == PendingExceptionSlotStatus.Approved || next == PendingExceptionSlotStatus.Withdrawn;
            case PendingExceptionSlotStatus.Approved:
                return next == PendingExceptionSlotStatus.Rejected;
            case PendingExceptionSlotStatus.Rejected:
                return next == PendingExceptionSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingExceptionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)BatchSize - Amount, 2);
    }

    public PendingExceptionSlot Copy()
    {
        return new PendingExceptionSlot(Code, Title, BatchSize, Version, Visible, Limit, Team, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
