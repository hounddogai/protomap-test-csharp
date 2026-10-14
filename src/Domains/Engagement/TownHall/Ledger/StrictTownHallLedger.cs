namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict town hall ledger is in its life cycle.</summary>
public enum StrictTownHallLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict town hall ledger. Its code is unique within a tenant.</summary>
public sealed class StrictTownHallLedger(string code, string name, bool approved, string source, string costCenter, bool visible, int leaveDays, StrictTownHallLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public string CostCenter { get; set; } = costCenter;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public StrictTownHallLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a strict town hall ledger in status requested.</summary>
    public static StrictTownHallLedger NewStrictTownHallLedger(string code, string name)
    {
        return new StrictTownHallLedger(code, name, false, "", "", false, 0, StrictTownHallLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTownHallLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTownHallLedgerStatus next)
    {
        switch (Status)
        {
            case StrictTownHallLedgerStatus.Requested:
                switch (next)
                {
                    case StrictTownHallLedgerStatus.Granted:
                        return true;
                }
                return false;
            case StrictTownHallLedgerStatus.Granted:
                return next == StrictTownHallLedgerStatus.Revoked || next == StrictTownHallLedgerStatus.Expired;
            case StrictTownHallLedgerStatus.Revoked:
                return next == StrictTownHallLedgerStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictTownHallLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict town hall ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public StrictTownHallLedger Copy()
    {
        return new StrictTownHallLedger(Code, Name, Approved, Source, CostCenter, Visible, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
