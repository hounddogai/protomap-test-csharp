namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit slot is in its life cycle.</summary>
public enum AuditSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One audit slot, with what the planners need to act on it.</summary>
public sealed class AuditSlot(string code, string title, int durationMinutes, string unit, int attempts, string externalRef, double maximum, AuditSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Unit { get; set; } = unit;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public AuditSlotStatus Status { get; set; } = status;

    /// <summary>Starts a audit slot in status proposed.</summary>
    public static AuditSlot NewAuditSlot(string code, string title)
    {
        return new AuditSlot(code, title, 0, "", 0, "", 0.0, AuditSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditSlotStatus next)
    {
        switch (Status)
        {
            case AuditSlotStatus.Proposed:
                switch (next)
                {
                    case AuditSlotStatus.Rejected:
                        return true;
                }
                return false;
            case AuditSlotStatus.Approved:
                return next == AuditSlotStatus.Rejected || next == AuditSlotStatus.Withdrawn;
            case AuditSlotStatus.Rejected:
                switch (next)
                {
                    case AuditSlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit slot"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)DurationMinutes * (double)Attempts, 2);
    }

    public AuditSlot Copy()
    {
        return new AuditSlot(Code, Title, DurationMinutes, Unit, Attempts, ExternalRef, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
