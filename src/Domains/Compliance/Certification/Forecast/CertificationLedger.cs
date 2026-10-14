namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification ledger is in its life cycle.</summary>
public enum CertificationLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification ledger. Its code is unique within a tenant.</summary>
public sealed class CertificationLedger(string code, string name, int updatedAt, double amount, string category, double margin, double rate, double threshold, CertificationLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public CertificationLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a certification ledger in status proposed.</summary>
    public static CertificationLedger NewCertificationLedger(string code, string name)
    {
        return new CertificationLedger(code, name, 0, 0.0, "", 0.0, 0.0, 0.0, CertificationLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationLedgerStatus next)
    {
        switch (Status)
        {
            case CertificationLedgerStatus.Proposed:
                switch (next)
                {
                    case CertificationLedgerStatus.Approved:
                    case CertificationLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case CertificationLedgerStatus.Approved:
                return next == CertificationLedgerStatus.Withdrawn;
            case CertificationLedgerStatus.Rejected:
                switch (next)
                {
                    case CertificationLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"certification ledger"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin + Rate, 2);
    }

    public CertificationLedger Copy()
    {
        return new CertificationLedger(Code, Name, UpdatedAt, Amount, Category, Margin, Rate, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
