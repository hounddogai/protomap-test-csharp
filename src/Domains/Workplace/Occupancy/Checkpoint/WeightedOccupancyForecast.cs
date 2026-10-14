namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted occupancy forecast is in its life cycle.</summary>
public enum WeightedOccupancyForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted occupancy forecast as the occupancy team keeps it.</summary>
public sealed class WeightedOccupancyForecast(string code, string name, string skill, bool pinned, int updatedAt, double threshold, int windowDays, double ratio, WeightedOccupancyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Threshold { get; set; } = threshold;
    public int WindowDays { get; set; } = windowDays;
    public double Ratio { get; set; } = ratio;
    public WeightedOccupancyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a weighted occupancy forecast in status open.</summary>
    public static WeightedOccupancyForecast NewWeightedOccupancyForecast(string code, string name)
    {
        return new WeightedOccupancyForecast(
            code,
            name,
            "",
            false,
            0,
            0.0,
            0,
            0.0,
            WeightedOccupancyForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedOccupancyForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedOccupancyForecastStatus next)
    {
        switch (Status)
        {
            case WeightedOccupancyForecastStatus.Open:
                switch (next)
                {
                    case WeightedOccupancyForecastStatus.InReview:
                    case WeightedOccupancyForecastStatus.Archived:
                        return true;
                }
                return false;
            case WeightedOccupancyForecastStatus.InReview:
                return next == WeightedOccupancyForecastStatus.Resolved;
            case WeightedOccupancyForecastStatus.Resolved:
                return next == WeightedOccupancyForecastStatus.Archived;
            case WeightedOccupancyForecastStatus.Reopened:
                return next == WeightedOccupancyForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedOccupancyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted occupancy forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted occupancy forecast"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)WindowDays * Threshold, 2);
    }

    public WeightedOccupancyForecast Copy()
    {
        return new WeightedOccupancyForecast(
            Code,
            Name,
            Skill,
            Pinned,
            UpdatedAt,
            Threshold,
            WindowDays,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
