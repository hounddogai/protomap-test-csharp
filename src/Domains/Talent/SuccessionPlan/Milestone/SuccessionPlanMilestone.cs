namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan milestone is in its life cycle.</summary>
public enum SuccessionPlanMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One succession plan milestone, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanMilestone(string code, string name, int limit, bool pinned, string checksum, string phone, int capacity, string source, string grade, bool active, SuccessionPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public string Source { get; set; } = source;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public SuccessionPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan milestone in status requested.</summary>
    public static SuccessionPlanMilestone NewSuccessionPlanMilestone(string code, string name)
    {
        return new SuccessionPlanMilestone(
            code,
            name,
            0,
            false,
            "",
            "",
            0,
            "",
            "",
            false,
            SuccessionPlanMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanMilestoneStatus.Requested:
                return next == SuccessionPlanMilestoneStatus.Granted || next == SuccessionPlanMilestoneStatus.Revoked;
            case SuccessionPlanMilestoneStatus.Granted:
                switch (next)
                {
                    case SuccessionPlanMilestoneStatus.Revoked:
                    case SuccessionPlanMilestoneStatus.Expired:
                        return true;
                }
                return false;
            case SuccessionPlanMilestoneStatus.Revoked:
                switch (next)
                {
                    case SuccessionPlanMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"succession plan milestone"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit + (double)Capacity, 2);
    }

    public SuccessionPlanMilestone Copy()
    {
        return new SuccessionPlanMilestone(
            Code,
            Name,
            Limit,
            Pinned,
            Checksum,
            Phone,
            Capacity,
            Source,
            Grade,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
