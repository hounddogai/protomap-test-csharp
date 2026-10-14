namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate cycle is in its life cycle.</summary>
public enum WeightedRateCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One weighted rate cycle, with what the planners need to act on it.</summary>
public sealed class WeightedRateCycle(string code, string name, int createdAt, double margin, string team, WeightedRateCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public string Team { get; set; } = team;
    public WeightedRateCycleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate cycle in status planned.</summary>
    public static WeightedRateCycle NewWeightedRateCycle(string code, string name)
    {
        return new WeightedRateCycle(code, name, 0, 0.0, "", WeightedRateCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRateCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRateCycleStatus next)
    {
        switch (Status)
        {
            case WeightedRateCycleStatus.Planned:
                switch (next)
                {
                    case WeightedRateCycleStatus.Scheduled:
                    case WeightedRateCycleStatus.InProgress:
                        return true;
                }
                return false;
            case WeightedRateCycleStatus.Scheduled:
                return next == WeightedRateCycleStatus.InProgress || next == WeightedRateCycleStatus.Completed;
            case WeightedRateCycleStatus.InProgress:
                switch (next)
                {
                    case WeightedRateCycleStatus.Completed:
                    case WeightedRateCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedRateCycleStatus.Completed:
                return next == WeightedRateCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedRateCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt - Margin, 2);
    }

    public WeightedRateCycle Copy()
    {
        return new WeightedRateCycle(Code, Name, CreatedAt, Margin, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
