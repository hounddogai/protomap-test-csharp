namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched policy ack ledger is in its life cycle.</summary>
public enum BatchedPolicyAckLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched policy ack ledger as the policy ack team keeps it.</summary>
public sealed class BatchedPolicyAckLedger(string code, string title, double fte, int version, double minimum, double amount, int leaveDays, BatchedPolicyAckLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public double Amount { get; set; } = amount;
    public int LeaveDays { get; set; } = leaveDays;
    public BatchedPolicyAckLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched policy ack ledger in status proposed.</summary>
    public static BatchedPolicyAckLedger NewBatchedPolicyAckLedger(string code, string title)
    {
        return new BatchedPolicyAckLedger(code, title, 0.0, 0, 0.0, 0.0, 0, BatchedPolicyAckLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPolicyAckLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPolicyAckLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedPolicyAckLedgerStatus.Proposed:
                switch (next)
                {
                    case BatchedPolicyAckLedgerStatus.Rejected:
                    case BatchedPolicyAckLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedPolicyAckLedgerStatus.Approved:
                switch (next)
                {
                    case BatchedPolicyAckLedgerStatus.Rejected:
                    case BatchedPolicyAckLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedPolicyAckLedgerStatus.Rejected:
                return next == BatchedPolicyAckLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedPolicyAckLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched policy ack ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)LeaveDays * (double)Version, 2);
    }

    public BatchedPolicyAckLedger Copy()
    {
        return new BatchedPolicyAckLedger(Code, Title, Fte, Version, Minimum, Amount, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
