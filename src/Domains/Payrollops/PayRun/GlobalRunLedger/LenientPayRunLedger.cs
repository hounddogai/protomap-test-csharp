namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay run ledger is in its life cycle.</summary>
public enum LenientPayRunLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient pay run ledger. Its code is unique within a tenant.</summary>
public sealed class LenientPayRunLedger(string code, string name, bool approved, int endsAt, string description, int retries, int createdAt, double ratio, LenientPayRunLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int EndsAt { get; set; } = endsAt;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public LenientPayRunLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay run ledger in status proposed.</summary>
    public static LenientPayRunLedger NewLenientPayRunLedger(string code, string name)
    {
        return new LenientPayRunLedger(code, name, false, 0, "", 0, 0, 0.0, LenientPayRunLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayRunLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayRunLedgerStatus next)
    {
        switch (Status)
        {
            case LenientPayRunLedgerStatus.Proposed:
                return next == LenientPayRunLedgerStatus.Rejected;
            case LenientPayRunLedgerStatus.Approved:
                switch (next)
                {
                    case LenientPayRunLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case LenientPayRunLedgerStatus.Rejected:
                return next == LenientPayRunLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientPayRunLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay run ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ratio + (double)EndsAt, 2);
    }

    public LenientPayRunLedger Copy()
    {
        return new LenientPayRunLedger(Code, Name, Approved, EndsAt, Description, Retries, CreatedAt, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
