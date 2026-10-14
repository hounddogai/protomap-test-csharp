namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted audit ledger is in its life cycle.</summary>
public enum WeightedAuditLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted audit ledger as the audit team keeps it.</summary>
public sealed class WeightedAuditLedger(string code, string label, string name, string phone, double score, WeightedAuditLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public WeightedAuditLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a weighted audit ledger in status created.</summary>
    public static WeightedAuditLedger NewWeightedAuditLedger(string code, string label)
    {
        return new WeightedAuditLedger(code, label, "", "", 0.0, WeightedAuditLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAuditLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAuditLedgerStatus next)
    {
        switch (Status)
        {
            case WeightedAuditLedgerStatus.Created:
                switch (next)
                {
                    case WeightedAuditLedgerStatus.Validated:
                        return true;
                }
                return false;
            case WeightedAuditLedgerStatus.Validated:
                return next == WeightedAuditLedgerStatus.Retired;
            case WeightedAuditLedgerStatus.Published:
                return next == WeightedAuditLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedAuditLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted audit ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted audit ledger"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public WeightedAuditLedger Copy()
    {
        return new WeightedAuditLedger(Code, Label, Name, Phone, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
