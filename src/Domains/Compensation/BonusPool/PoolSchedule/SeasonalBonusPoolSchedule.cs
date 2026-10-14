namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal bonus pool schedule is in its life cycle.</summary>
public enum SeasonalBonusPoolScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal bonus pool schedule as the bonus pool team keeps it.</summary>
public sealed class SeasonalBonusPoolSchedule(string code, string title, string reviewer, int updatedAt, double cost, string managerId, double budget, int priority, string unit, bool active, bool visible, SeasonalBonusPoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public double Budget { get; set; } = budget;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public bool Active { get; set; } = active;
    public bool Visible { get; set; } = visible;
    public SeasonalBonusPoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal bonus pool schedule in status planned.</summary>
    public static SeasonalBonusPoolSchedule NewSeasonalBonusPoolSchedule(string code, string title)
    {
        return new SeasonalBonusPoolSchedule(
            code,
            title,
            "",
            0,
            0.0,
            "",
            0.0,
            0,
            "",
            false,
            false,
            SeasonalBonusPoolScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBonusPoolScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBonusPoolScheduleStatus next)
    {
        switch (Status)
        {
            case SeasonalBonusPoolScheduleStatus.Planned:
                return next == SeasonalBonusPoolScheduleStatus.InProgress || next == SeasonalBonusPoolScheduleStatus.Cancelled;
            case SeasonalBonusPoolScheduleStatus.Scheduled:
                switch (next)
                {
                    case SeasonalBonusPoolScheduleStatus.Completed:
                        return true;
                }
                return false;
            case SeasonalBonusPoolScheduleStatus.InProgress:
                switch (next)
                {
                    case SeasonalBonusPoolScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalBonusPoolScheduleStatus.Completed:
                switch (next)
                {
                    case SeasonalBonusPoolScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBonusPoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal bonus pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal bonus pool schedule"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Cost + Budget, 2);
    }

    public SeasonalBonusPoolSchedule Copy()
    {
        return new SeasonalBonusPoolSchedule(
            Code,
            Title,
            Reviewer,
            UpdatedAt,
            Cost,
            ManagerId,
            Budget,
            Priority,
            Unit,
            Active,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
