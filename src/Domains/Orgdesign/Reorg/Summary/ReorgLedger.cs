namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg ledger is in its life cycle.</summary>
public enum ReorgLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A reorg ledger as the reorg team keeps it.</summary>
public sealed class ReorgLedger(string code, string name, bool approved, string title, string salaryBand, double score, ReorgLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Score { get; set; } = score;
    public ReorgLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a reorg ledger in status open.</summary>
    public static ReorgLedger NewReorgLedger(string code, string name)
    {
        return new ReorgLedger(code, name, false, "", "", 0.0, ReorgLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgLedgerStatus next)
    {
        switch (Status)
        {
            case ReorgLedgerStatus.Open:
                switch (next)
                {
                    case ReorgLedgerStatus.InReview:
                    case ReorgLedgerStatus.Resolved:
                        return true;
                }
                return false;
            case ReorgLedgerStatus.InReview:
                return next == ReorgLedgerStatus.Resolved;
            case ReorgLedgerStatus.Resolved:
                return next == ReorgLedgerStatus.Archived;
            case ReorgLedgerStatus.Reopened:
                return next == ReorgLedgerStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ReorgLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public ReorgLedger Copy()
    {
        return new ReorgLedger(Code, Name, Approved, Title, SalaryBand, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
