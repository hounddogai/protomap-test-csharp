namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay run ledger is in its life cycle.</summary>
public enum CappedPayRunLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped pay run ledger, with what the planners need to act on it.</summary>
public sealed class CappedPayRunLedger(string code, string name, int priority, double ratio, bool archived, string channel, bool pinned, int leaveDays, string contactEmail, string region, int batchSize, CappedPayRunLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Ratio { get; set; } = ratio;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public int BatchSize { get; set; } = batchSize;
    public CappedPayRunLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay run ledger in status proposed.</summary>
    public static CappedPayRunLedger NewCappedPayRunLedger(string code, string name)
    {
        return new CappedPayRunLedger(
            code,
            name,
            0,
            0.0,
            false,
            "",
            false,
            0,
            "",
            "",
            0,
            CappedPayRunLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayRunLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayRunLedgerStatus next)
    {
        switch (Status)
        {
            case CappedPayRunLedgerStatus.Proposed:
                return next == CappedPayRunLedgerStatus.Rejected || next == CappedPayRunLedgerStatus.Withdrawn;
            case CappedPayRunLedgerStatus.Approved:
                return next == CappedPayRunLedgerStatus.Withdrawn;
            case CappedPayRunLedgerStatus.Rejected:
                switch (next)
                {
                    case CappedPayRunLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPayRunLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay run ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)BatchSize + (double)LeaveDays, 2);
    }

    public CappedPayRunLedger Copy()
    {
        return new CappedPayRunLedger(
            Code,
            Name,
            Priority,
            Ratio,
            Archived,
            Channel,
            Pinned,
            LeaveDays,
            ContactEmail,
            Region,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
