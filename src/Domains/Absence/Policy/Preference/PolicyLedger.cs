namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ledger is in its life cycle.</summary>
public enum PolicyLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a policy ledger and its current status.</summary>
public sealed class PolicyLedger(string code, string title, double cost, int endsAt, string checksum, int capacity, PolicyLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public int Capacity { get; set; } = capacity;
    public PolicyLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a policy ledger in status planned.</summary>
    public static PolicyLedger NewPolicyLedger(string code, string title)
    {
        return new PolicyLedger(code, title, 0.0, 0, "", 0, PolicyLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyLedgerStatus next)
    {
        switch (Status)
        {
            case PolicyLedgerStatus.Planned:
                return next == PolicyLedgerStatus.InProgress;
            case PolicyLedgerStatus.Scheduled:
                return next == PolicyLedgerStatus.InProgress || next == PolicyLedgerStatus.Completed;
            case PolicyLedgerStatus.InProgress:
                switch (next)
                {
                    case PolicyLedgerStatus.Completed:
                    case PolicyLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyLedgerStatus.Completed:
                switch (next)
                {
                    case PolicyLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Cost * (double)EndsAt, 2);
    }

    public PolicyLedger Copy()
    {
        return new PolicyLedger(Code, Title, Cost, EndsAt, Checksum, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
