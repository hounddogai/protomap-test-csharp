namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit record is in its life cycle.</summary>
public enum AuditRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A audit record as the audit team keeps it.</summary>
public sealed class AuditRecord(string code, string label, string reviewer, double rate, string locale, string region, int createdAt, double margin, int limit, string notes, string segment, AuditRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public string Segment { get; set; } = segment;
    public AuditRecordStatus Status { get; set; } = status;

    /// <summary>Starts a audit record in status created.</summary>
    public static AuditRecord NewAuditRecord(string code, string label)
    {
        return new AuditRecord(code, label, "", 0.0, "", "", 0, 0.0, 0, "", "", AuditRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditRecordStatus next)
    {
        switch (Status)
        {
            case AuditRecordStatus.Created:
                switch (next)
                {
                    case AuditRecordStatus.Validated:
                        return true;
                }
                return false;
            case AuditRecordStatus.Validated:
                switch (next)
                {
                    case AuditRecordStatus.Published:
                        return true;
                }
                return false;
            case AuditRecordStatus.Published:
                switch (next)
                {
                    case AuditRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"audit record"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)CreatedAt * Margin, 2);
    }

    public AuditRecord Copy()
    {
        return new AuditRecord(
            Code,
            Label,
            Reviewer,
            Rate,
            Locale,
            Region,
            CreatedAt,
            Margin,
            Limit,
            Notes,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
