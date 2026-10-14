namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered career path forecast is in its life cycle.</summary>
public enum TieredCareerPathForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered career path forecast and its current status.</summary>
public sealed class TieredCareerPathForecast(string code, string name, int attempts, string description, string region, Dictionary<string, string> attributes, double cost, int graceDays, bool approved, TieredCareerPathForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public TieredCareerPathForecastStatus Status { get; set; } = status;

    /// <summary>Starts a tiered career path forecast in status created.</summary>
    public static TieredCareerPathForecast NewTieredCareerPathForecast(string code, string name)
    {
        return new TieredCareerPathForecast(
            code,
            name,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            false,
            TieredCareerPathForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCareerPathForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCareerPathForecastStatus next)
    {
        switch (Status)
        {
            case TieredCareerPathForecastStatus.Created:
                switch (next)
                {
                    case TieredCareerPathForecastStatus.Published:
                    case TieredCareerPathForecastStatus.Retired:
                        return true;
                }
                return false;
            case TieredCareerPathForecastStatus.Validated:
                return next == TieredCareerPathForecastStatus.Published || next == TieredCareerPathForecastStatus.Retired;
            case TieredCareerPathForecastStatus.Published:
                return next == TieredCareerPathForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredCareerPathForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered career path forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double NetValue()
    {
        return Math.Round(Cost - (double)GraceDays, 2);
    }

    public TieredCareerPathForecast Copy()
    {
        return new TieredCareerPathForecast(
            Code,
            Name,
            Attempts,
            Description,
            Region,
            Attributes,
            Cost,
            GraceDays,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
