namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool cycle is in its life cycle.</summary>
public enum PoolCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pool cycle as the pool team keeps it.</summary>
public sealed class PoolCycle(string code, string title, string team, string category, string skill, string externalRef, int retries, List<string> tags, double budget, string country, PoolCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Category { get; set; } = category;
    public string Skill { get; set; } = skill;
    public string ExternalRef { get; set; } = externalRef;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public PoolCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pool cycle in status planned.</summary>
    public static PoolCycle NewPoolCycle(string code, string title)
    {
        return new PoolCycle(code, title, "", "", "", "", 0, new List<string>(), 0.0, "", PoolCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolCycleStatus next)
    {
        switch (Status)
        {
            case PoolCycleStatus.Planned:
                return next == PoolCycleStatus.Scheduled;
            case PoolCycleStatus.Scheduled:
                return next == PoolCycleStatus.InProgress || next == PoolCycleStatus.Cancelled;
            case PoolCycleStatus.InProgress:
                return next == PoolCycleStatus.Completed;
            case PoolCycleStatus.Completed:
                switch (next)
                {
                    case PoolCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round(Budget + (double)Retries, 2);
    }

    public PoolCycle Copy()
    {
        return new PoolCycle(
            Code,
            Title,
            Team,
            Category,
            Skill,
            ExternalRef,
            Retries,
            Tags,
            Budget,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
