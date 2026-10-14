namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request rule is in its life cycle.</summary>
public enum LeaveRequestRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a leave request rule and its current status.</summary>
public sealed class LeaveRequestRule(string code, string label, int capacity, string description, double maximum, int retries, double score, double amount, string country, LeaveRequestRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public int Retries { get; set; } = retries;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public string Country { get; set; } = country;
    public LeaveRequestRuleStatus Status { get; set; } = status;

    /// <summary>Starts a leave request rule in status idle.</summary>
    public static LeaveRequestRule NewLeaveRequestRule(string code, string label)
    {
        return new LeaveRequestRule(code, label, 0, "", 0.0, 0, 0.0, 0.0, "", LeaveRequestRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestRuleStatus next)
    {
        switch (Status)
        {
            case LeaveRequestRuleStatus.Idle:
                switch (next)
                {
                    case LeaveRequestRuleStatus.Ready:
                    case LeaveRequestRuleStatus.Draining:
                        return true;
                }
                return false;
            case LeaveRequestRuleStatus.Warming:
                switch (next)
                {
                    case LeaveRequestRuleStatus.Ready:
                        return true;
                }
                return false;
            case LeaveRequestRuleStatus.Ready:
                return next == LeaveRequestRuleStatus.Stopped;
            case LeaveRequestRuleStatus.Draining:
                switch (next)
                {
                    case LeaveRequestRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount * (double)Capacity, 2);
    }

    public LeaveRequestRule Copy()
    {
        return new LeaveRequestRule(
            Code,
            Label,
            Capacity,
            Description,
            Maximum,
            Retries,
            Score,
            Amount,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
