namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit entry is in its life cycle.</summary>
public enum AuditEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a audit entry and its current status.</summary>
public sealed class AuditEntry(string code, string label, string badge, string notes, string description, string currency, double cost, string name, AuditEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public string Currency { get; set; } = currency;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public AuditEntryStatus Status { get; set; } = status;

    /// <summary>Starts a audit entry in status proposed.</summary>
    public static AuditEntry NewAuditEntry(string code, string label)
    {
        return new AuditEntry(code, label, "", "", "", "", 0.0, "", AuditEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditEntryStatus next)
    {
        switch (Status)
        {
            case AuditEntryStatus.Proposed:
                switch (next)
                {
                    case AuditEntryStatus.Rejected:
                        return true;
                }
                return false;
            case AuditEntryStatus.Approved:
                switch (next)
                {
                    case AuditEntryStatus.Rejected:
                        return true;
                }
                return false;
            case AuditEntryStatus.Rejected:
                switch (next)
                {
                    case AuditEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"audit entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public AuditEntry Copy()
    {
        return new AuditEntry(Code, Label, Badge, Notes, Description, Currency, Cost, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
