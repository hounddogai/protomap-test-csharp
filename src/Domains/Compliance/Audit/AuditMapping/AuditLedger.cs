namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit ledger is in its life cycle.</summary>
public enum AuditLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One audit ledger, with what the planners need to act on it.</summary>
public sealed class AuditLedger(string code, string name, int endsAt, string description, double weight, string locale, double hours, string jobCode, double ceiling, int retries, string contactEmail, AuditLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public string ContactEmail { get; set; } = contactEmail;
    public AuditLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a audit ledger in status open.</summary>
    public static AuditLedger NewAuditLedger(string code, string name)
    {
        return new AuditLedger(code, name, 0, "", 0.0, "", 0.0, "", 0.0, 0, "", AuditLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditLedgerStatus next)
    {
        switch (Status)
        {
            case AuditLedgerStatus.Open:
                switch (next)
                {
                    case AuditLedgerStatus.Archived:
                        return true;
                }
                return false;
            case AuditLedgerStatus.InReview:
                switch (next)
                {
                    case AuditLedgerStatus.Resolved:
                    case AuditLedgerStatus.Archived:
                        return true;
                }
                return false;
            case AuditLedgerStatus.Resolved:
                return next == AuditLedgerStatus.Archived;
            case AuditLedgerStatus.Reopened:
                switch (next)
                {
                    case AuditLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"audit ledger"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Retries + (double)EndsAt, 2);
    }

    public AuditLedger Copy()
    {
        return new AuditLedger(
            Code,
            Name,
            EndsAt,
            Description,
            Weight,
            Locale,
            Hours,
            JobCode,
            Ceiling,
            Retries,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
