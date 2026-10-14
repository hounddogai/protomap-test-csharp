namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted headcount schedule is in its life cycle.</summary>
public enum WeightedHeadcountScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One weighted headcount schedule, with what the planners need to act on it.</summary>
public sealed class WeightedHeadcountSchedule(string code, string name, int batchSize, string team, int leaveDays, int endsAt, WeightedHeadcountScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public int LeaveDays { get; set; } = leaveDays;
    public int EndsAt { get; set; } = endsAt;
    public WeightedHeadcountScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted headcount schedule in status draft.</summary>
    public static WeightedHeadcountSchedule NewWeightedHeadcountSchedule(string code, string name)
    {
        return new WeightedHeadcountSchedule(code, name, 0, "", 0, 0, WeightedHeadcountScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedHeadcountScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedHeadcountScheduleStatus next)
    {
        switch (Status)
        {
            case WeightedHeadcountScheduleStatus.Draft:
                return next == WeightedHeadcountScheduleStatus.Pending || next == WeightedHeadcountScheduleStatus.Active;
            case WeightedHeadcountScheduleStatus.Pending:
                switch (next)
                {
                    case WeightedHeadcountScheduleStatus.Closed:
                        return true;
                }
                return false;
            case WeightedHeadcountScheduleStatus.Active:
                switch (next)
                {
                    case WeightedHeadcountScheduleStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedHeadcountScheduleStatus.Suspended:
                switch (next)
                {
                    case WeightedHeadcountScheduleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedHeadcountScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted headcount schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted headcount schedule"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt * (double)BatchSize, 2);
    }

    public WeightedHeadcountSchedule Copy()
    {
        return new WeightedHeadcountSchedule(Code, Name, BatchSize, Team, LeaveDays, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
