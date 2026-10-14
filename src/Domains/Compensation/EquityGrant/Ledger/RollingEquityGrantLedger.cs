namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling equity grant ledger is in its life cycle.</summary>
public enum RollingEquityGrantLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling equity grant ledger as the equity grant team keeps it.</summary>
public sealed class RollingEquityGrantLedger(string code, string title, bool active, int limit, double ratio, RollingEquityGrantLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int Limit { get; set; } = limit;
    public double Ratio { get; set; } = ratio;
    public RollingEquityGrantLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rolling equity grant ledger in status draft.</summary>
    public static RollingEquityGrantLedger NewRollingEquityGrantLedger(string code, string title)
    {
        return new RollingEquityGrantLedger(code, title, false, 0, 0.0, RollingEquityGrantLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingEquityGrantLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingEquityGrantLedgerStatus next)
    {
        switch (Status)
        {
            case RollingEquityGrantLedgerStatus.Draft:
                switch (next)
                {
                    case RollingEquityGrantLedgerStatus.Active:
                    case RollingEquityGrantLedgerStatus.Closed:
                        return true;
                }
                return false;
            case RollingEquityGrantLedgerStatus.Pending:
                return next == RollingEquityGrantLedgerStatus.Active || next == RollingEquityGrantLedgerStatus.Closed;
            case RollingEquityGrantLedgerStatus.Active:
                return next == RollingEquityGrantLedgerStatus.Suspended || next == RollingEquityGrantLedgerStatus.Closed;
            case RollingEquityGrantLedgerStatus.Suspended:
                return next == RollingEquityGrantLedgerStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingEquityGrantLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling equity grant ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round(Ratio * (double)Limit, 2);
    }

    public RollingEquityGrantLedger Copy()
    {
        return new RollingEquityGrantLedger(Code, Title, Active, Limit, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
