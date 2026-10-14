namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack ledger is in its life cycle.</summary>
public enum PolicyAckLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a policy ack ledger and its current status.</summary>
public sealed class PolicyAckLedger(string code, string name, string reviewer, int batchSize, double hours, string label, string ownerId, string skill, int graceDays, PolicyAckLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public int GraceDays { get; set; } = graceDays;
    public PolicyAckLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack ledger in status requested.</summary>
    public static PolicyAckLedger NewPolicyAckLedger(string code, string name)
    {
        return new PolicyAckLedger(code, name, "", 0, 0.0, "", "", "", 0, PolicyAckLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckLedgerStatus next)
    {
        switch (Status)
        {
            case PolicyAckLedgerStatus.Requested:
                switch (next)
                {
                    case PolicyAckLedgerStatus.Revoked:
                        return true;
                }
                return false;
            case PolicyAckLedgerStatus.Granted:
                return next == PolicyAckLedgerStatus.Revoked || next == PolicyAckLedgerStatus.Expired;
            case PolicyAckLedgerStatus.Revoked:
                return next == PolicyAckLedgerStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PolicyAckLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy ack ledger"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays + Hours, 2);
    }

    public PolicyAckLedger Copy()
    {
        return new PolicyAckLedger(Code, Name, Reviewer, BatchSize, Hours, Label, OwnerId, Skill, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
