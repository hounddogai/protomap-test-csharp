namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted balance schedule is in its life cycle.</summary>
public enum WeightedBalanceScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One weighted balance schedule, with what the planners need to act on it.</summary>
public sealed class WeightedBalanceSchedule(string code, string title, int batchSize, int version, Dictionary<string, string> attributes, string skill, string unit, string source, int retries, string region, WeightedBalanceScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public WeightedBalanceScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted balance schedule in status draft.</summary>
    public static WeightedBalanceSchedule NewWeightedBalanceSchedule(string code, string title)
    {
        return new WeightedBalanceSchedule(
            code,
            title,
            0,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0,
            "",
            WeightedBalanceScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBalanceScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBalanceScheduleStatus next)
    {
        switch (Status)
        {
            case WeightedBalanceScheduleStatus.Draft:
                switch (next)
                {
                    case WeightedBalanceScheduleStatus.Pending:
                    case WeightedBalanceScheduleStatus.Closed:
                        return true;
                }
                return false;
            case WeightedBalanceScheduleStatus.Pending:
                return next == WeightedBalanceScheduleStatus.Active;
            case WeightedBalanceScheduleStatus.Active:
                switch (next)
                {
                    case WeightedBalanceScheduleStatus.Suspended:
                    case WeightedBalanceScheduleStatus.Closed:
                        return true;
                }
                return false;
            case WeightedBalanceScheduleStatus.Suspended:
                return next == WeightedBalanceScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedBalanceScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted balance schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"weighted balance schedule"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version * (double)BatchSize, 2);
    }

    public WeightedBalanceSchedule Copy()
    {
        return new WeightedBalanceSchedule(
            Code,
            Title,
            BatchSize,
            Version,
            Attributes,
            Skill,
            Unit,
            Source,
            Retries,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
