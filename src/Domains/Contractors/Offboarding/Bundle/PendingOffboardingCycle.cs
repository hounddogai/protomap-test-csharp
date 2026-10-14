namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending offboarding cycle is in its life cycle.</summary>
public enum PendingOffboardingCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pending offboarding cycle. Its code is unique within a tenant.</summary>
public sealed class PendingOffboardingCycle(string code, string label, double amount, double score, double budget, double rate, string name, string managerId, string description, string channel, PendingOffboardingCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public PendingOffboardingCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pending offboarding cycle in status open.</summary>
    public static PendingOffboardingCycle NewPendingOffboardingCycle(string code, string label)
    {
        return new PendingOffboardingCycle(
            code,
            label,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            PendingOffboardingCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingOffboardingCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingOffboardingCycleStatus next)
    {
        switch (Status)
        {
            case PendingOffboardingCycleStatus.Open:
                switch (next)
                {
                    case PendingOffboardingCycleStatus.Reopened:
                    case PendingOffboardingCycleStatus.Archived:
                        return true;
                }
                return false;
            case PendingOffboardingCycleStatus.InReview:
                switch (next)
                {
                    case PendingOffboardingCycleStatus.Resolved:
                        return true;
                }
                return false;
            case PendingOffboardingCycleStatus.Resolved:
                return next == PendingOffboardingCycleStatus.Reopened || next == PendingOffboardingCycleStatus.Archived;
            case PendingOffboardingCycleStatus.Reopened:
                switch (next)
                {
                    case PendingOffboardingCycleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingOffboardingCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending offboarding cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(Budget - Score, 2);
    }

    public PendingOffboardingCycle Copy()
    {
        return new PendingOffboardingCycle(
            Code,
            Label,
            Amount,
            Score,
            Budget,
            Rate,
            Name,
            ManagerId,
            Description,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
