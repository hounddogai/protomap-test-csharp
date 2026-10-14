namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal town hall metric is in its life cycle.</summary>
public enum SeasonalTownHallMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal town hall metric. Its code is unique within a tenant.</summary>
public sealed class SeasonalTownHallMetric(string code, string name, string locale, double budget, int startsAt, SeasonalTownHallMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public double Budget { get; set; } = budget;
    public int StartsAt { get; set; } = startsAt;
    public SeasonalTownHallMetricStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal town hall metric in status created.</summary>
    public static SeasonalTownHallMetric NewSeasonalTownHallMetric(string code, string name)
    {
        return new SeasonalTownHallMetric(code, name, "", 0.0, 0, SeasonalTownHallMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTownHallMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTownHallMetricStatus next)
    {
        switch (Status)
        {
            case SeasonalTownHallMetricStatus.Created:
                switch (next)
                {
                    case SeasonalTownHallMetricStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalTownHallMetricStatus.Validated:
                return next == SeasonalTownHallMetricStatus.Published || next == SeasonalTownHallMetricStatus.Retired;
            case SeasonalTownHallMetricStatus.Published:
                return next == SeasonalTownHallMetricStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalTownHallMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal town hall metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal town hall metric"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Budget - (double)StartsAt, 2);
    }

    public SeasonalTownHallMetric Copy()
    {
        return new SeasonalTownHallMetric(Code, Name, Locale, Budget, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
