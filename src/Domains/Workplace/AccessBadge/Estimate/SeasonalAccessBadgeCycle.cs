namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal access badge cycle is in its life cycle.</summary>
public enum SeasonalAccessBadgeCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One seasonal access badge cycle, with what the planners need to act on it.</summary>
public sealed class SeasonalAccessBadgeCycle(string code, string name, string title, int windowDays, string category, int priority, int batchSize, string reviewer, string costCenter, SeasonalAccessBadgeCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public SeasonalAccessBadgeCycleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal access badge cycle in status new.</summary>
    public static SeasonalAccessBadgeCycle NewSeasonalAccessBadgeCycle(string code, string name)
    {
        return new SeasonalAccessBadgeCycle(code, name, "", 0, "", 0, 0, "", "", SeasonalAccessBadgeCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccessBadgeCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccessBadgeCycleStatus next)
    {
        switch (Status)
        {
            case SeasonalAccessBadgeCycleStatus.New:
                switch (next)
                {
                    case SeasonalAccessBadgeCycleStatus.Queued:
                    case SeasonalAccessBadgeCycleStatus.Running:
                        return true;
                }
                return false;
            case SeasonalAccessBadgeCycleStatus.Queued:
                return next == SeasonalAccessBadgeCycleStatus.Done || next == SeasonalAccessBadgeCycleStatus.Failed;
            case SeasonalAccessBadgeCycleStatus.Running:
                return next == SeasonalAccessBadgeCycleStatus.Done || next == SeasonalAccessBadgeCycleStatus.Failed;
            case SeasonalAccessBadgeCycleStatus.Done:
                return next == SeasonalAccessBadgeCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalAccessBadgeCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal access badge cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal access badge cycle"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)BatchSize - (double)WindowDays, 2);
    }

    public SeasonalAccessBadgeCycle Copy()
    {
        return new SeasonalAccessBadgeCycle(
            Code,
            Name,
            Title,
            WindowDays,
            Category,
            Priority,
            BatchSize,
            Reviewer,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
