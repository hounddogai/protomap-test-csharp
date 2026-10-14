namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance milestone is in its life cycle.</summary>
public enum BalanceMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a balance milestone and its current status.</summary>
public sealed class BalanceMilestone(string code, string label, bool pinned, double minimum, double ratio, BalanceMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public BalanceMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a balance milestone in status created.</summary>
    public static BalanceMilestone NewBalanceMilestone(string code, string label)
    {
        return new BalanceMilestone(code, label, false, 0.0, 0.0, BalanceMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceMilestoneStatus next)
    {
        switch (Status)
        {
            case BalanceMilestoneStatus.Created:
                switch (next)
                {
                    case BalanceMilestoneStatus.Validated:
                    case BalanceMilestoneStatus.Retired:
                        return true;
                }
                return false;
            case BalanceMilestoneStatus.Validated:
                return next == BalanceMilestoneStatus.Published || next == BalanceMilestoneStatus.Retired;
            case BalanceMilestoneStatus.Published:
                return next == BalanceMilestoneStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BalanceMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"balance milestone"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ratio * Minimum, 2);
    }

    public BalanceMilestone Copy()
    {
        return new BalanceMilestone(Code, Label, Pinned, Minimum, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
