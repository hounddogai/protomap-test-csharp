namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pool milestone is in its life cycle.</summary>
public enum StrictPoolMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict pool milestone and its current status.</summary>
public sealed class StrictPoolMilestone(string code, string label, string badge, string jobCode, string channel, string title, double amount, StrictPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public StrictPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a strict pool milestone in status proposed.</summary>
    public static StrictPoolMilestone NewStrictPoolMilestone(string code, string label)
    {
        return new StrictPoolMilestone(code, label, "", "", "", "", 0.0, StrictPoolMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPoolMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case StrictPoolMilestoneStatus.Proposed:
                switch (next)
                {
                    case StrictPoolMilestoneStatus.Rejected:
                    case StrictPoolMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictPoolMilestoneStatus.Approved:
                switch (next)
                {
                    case StrictPoolMilestoneStatus.Rejected:
                    case StrictPoolMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictPoolMilestoneStatus.Rejected:
                switch (next)
                {
                    case StrictPoolMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict pool milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictPoolMilestone Copy()
    {
        return new StrictPoolMilestone(Code, Label, Badge, JobCode, Channel, Title, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
