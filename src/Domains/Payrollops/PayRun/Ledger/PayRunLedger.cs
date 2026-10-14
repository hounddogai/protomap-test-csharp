namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run ledger is in its life cycle.</summary>
public enum PayRunLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pay run ledger and its current status.</summary>
public sealed class PayRunLedger(string code, string name, string timezone, string source, string costCenter, string checksum, double amount, PayRunLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public string CostCenter { get; set; } = costCenter;
    public string Checksum { get; set; } = checksum;
    public double Amount { get; set; } = amount;
    public PayRunLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a pay run ledger in status created.</summary>
    public static PayRunLedger NewPayRunLedger(string code, string name)
    {
        return new PayRunLedger(code, name, "", "", "", "", 0.0, PayRunLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunLedgerStatus next)
    {
        switch (Status)
        {
            case PayRunLedgerStatus.Created:
                switch (next)
                {
                    case PayRunLedgerStatus.Validated:
                    case PayRunLedgerStatus.Retired:
                        return true;
                }
                return false;
            case PayRunLedgerStatus.Validated:
                switch (next)
                {
                    case PayRunLedgerStatus.Published:
                    case PayRunLedgerStatus.Retired:
                        return true;
                }
                return false;
            case PayRunLedgerStatus.Published:
                return next == PayRunLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PayRunLedger Copy()
    {
        return new PayRunLedger(Code, Name, Timezone, Source, CostCenter, Checksum, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
