namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending audit record is in its life cycle.</summary>
public enum PendingAuditRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pending audit record and its current status.</summary>
public sealed class PendingAuditRecord(string code, string title, string label, double ceiling, double minimum, PendingAuditRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public PendingAuditRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pending audit record in status draft.</summary>
    public static PendingAuditRecord NewPendingAuditRecord(string code, string title)
    {
        return new PendingAuditRecord(code, title, "", 0.0, 0.0, PendingAuditRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAuditRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAuditRecordStatus next)
    {
        switch (Status)
        {
            case PendingAuditRecordStatus.Draft:
                switch (next)
                {
                    case PendingAuditRecordStatus.Pending:
                        return true;
                }
                return false;
            case PendingAuditRecordStatus.Pending:
                switch (next)
                {
                    case PendingAuditRecordStatus.Closed:
                        return true;
                }
                return false;
            case PendingAuditRecordStatus.Active:
                switch (next)
                {
                    case PendingAuditRecordStatus.Suspended:
                        return true;
                }
                return false;
            case PendingAuditRecordStatus.Suspended:
                switch (next)
                {
                    case PendingAuditRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingAuditRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending audit record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(Minimum * Ceiling, 2);
    }

    public PendingAuditRecord Copy()
    {
        return new PendingAuditRecord(Code, Title, Label, Ceiling, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
