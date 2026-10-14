namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination plan is in its life cycle.</summary>
public enum NominationPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One nomination plan, with what the planners need to act on it.</summary>
public sealed class NominationPlan(string code, string label, string team, string name, string channel, double hours, double rate, string region, NominationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public double Rate { get; set; } = rate;
    public string Region { get; set; } = region;
    public NominationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a nomination plan in status planned.</summary>
    public static NominationPlan NewNominationPlan(string code, string label)
    {
        return new NominationPlan(code, label, "", "", "", 0.0, 0.0, "", NominationPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationPlanStatus next)
    {
        switch (Status)
        {
            case NominationPlanStatus.Planned:
                switch (next)
                {
                    case NominationPlanStatus.Completed:
                        return true;
                }
                return false;
            case NominationPlanStatus.Scheduled:
                switch (next)
                {
                    case NominationPlanStatus.InProgress:
                        return true;
                }
                return false;
            case NominationPlanStatus.InProgress:
                switch (next)
                {
                    case NominationPlanStatus.Completed:
                    case NominationPlanStatus.Cancelled:
                        return true;
                }
                return false;
            case NominationPlanStatus.Completed:
                switch (next)
                {
                    case NominationPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"nomination plan"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rate * Hours, 2);
    }

    public NominationPlan Copy()
    {
        return new NominationPlan(Code, Label, Team, Name, Channel, Hours, Rate, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
