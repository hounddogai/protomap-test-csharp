namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default bonus pool allocation is in its life cycle.</summary>
public enum DefaultBonusPoolAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default bonus pool allocation. Its code is unique within a tenant.</summary>
public sealed class DefaultBonusPoolAllocation(string code, string name, string description, int version, string team, int priority, int attempts, double maximum, string grade, DefaultBonusPoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public string Team { get; set; } = team;
    public int Priority { get; set; } = priority;
    public int Attempts { get; set; } = attempts;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public DefaultBonusPoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a default bonus pool allocation in status idle.</summary>
    public static DefaultBonusPoolAllocation NewDefaultBonusPoolAllocation(string code, string name)
    {
        return new DefaultBonusPoolAllocation(
            code,
            name,
            "",
            0,
            "",
            0,
            0,
            0.0,
            "",
            DefaultBonusPoolAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBonusPoolAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBonusPoolAllocationStatus next)
    {
        switch (Status)
        {
            case DefaultBonusPoolAllocationStatus.Idle:
                return next == DefaultBonusPoolAllocationStatus.Warming || next == DefaultBonusPoolAllocationStatus.Ready;
            case DefaultBonusPoolAllocationStatus.Warming:
                return next == DefaultBonusPoolAllocationStatus.Ready;
            case DefaultBonusPoolAllocationStatus.Ready:
                switch (next)
                {
                    case DefaultBonusPoolAllocationStatus.Draining:
                    case DefaultBonusPoolAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultBonusPoolAllocationStatus.Draining:
                return next == DefaultBonusPoolAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultBonusPoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default bonus pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"default bonus pool allocation"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Attempts * (double)Version, 2);
    }

    public DefaultBonusPoolAllocation Copy()
    {
        return new DefaultBonusPoolAllocation(
            Code,
            Name,
            Description,
            Version,
            Team,
            Priority,
            Attempts,
            Maximum,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
