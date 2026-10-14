namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy leave request audit is in its life cycle.</summary>
public enum LegacyLeaveRequestAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy leave request audit as the leave request team keeps it.</summary>
public sealed class LegacyLeaveRequestAudit(string code, string label, int retries, Dictionary<string, string> attributes, bool pinned, LegacyLeaveRequestAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Pinned { get; set; } = pinned;
    public LegacyLeaveRequestAuditStatus Status { get; set; } = status;

    /// <summary>Starts a legacy leave request audit in status draft.</summary>
    public static LegacyLeaveRequestAudit NewLegacyLeaveRequestAudit(string code, string label)
    {
        return new LegacyLeaveRequestAudit(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            false,
            LegacyLeaveRequestAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLeaveRequestAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLeaveRequestAuditStatus next)
    {
        switch (Status)
        {
            case LegacyLeaveRequestAuditStatus.Draft:
                switch (next)
                {
                    case LegacyLeaveRequestAuditStatus.Active:
                    case LegacyLeaveRequestAuditStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyLeaveRequestAuditStatus.Pending:
                return next == LegacyLeaveRequestAuditStatus.Active;
            case LegacyLeaveRequestAuditStatus.Active:
                return next == LegacyLeaveRequestAuditStatus.Suspended || next == LegacyLeaveRequestAuditStatus.Closed;
            case LegacyLeaveRequestAuditStatus.Suspended:
                switch (next)
                {
                    case LegacyLeaveRequestAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyLeaveRequestAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy leave request audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy leave request audit"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public LegacyLeaveRequestAudit Copy()
    {
        return new LegacyLeaveRequestAudit(Code, Label, Retries, Attributes, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
