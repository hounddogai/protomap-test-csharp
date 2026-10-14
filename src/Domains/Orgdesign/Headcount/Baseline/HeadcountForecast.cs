namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount forecast is in its life cycle.</summary>
public enum HeadcountForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A headcount forecast as the headcount team keeps it.</summary>
public sealed class HeadcountForecast(string code, string name, int windowDays, string reviewer, double fte, int startsAt, double margin, double cost, string salaryBand, double minimum, Dictionary<string, string> attributes, HeadcountForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public int StartsAt { get; set; } = startsAt;
    public double Margin { get; set; } = margin;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public HeadcountForecastStatus Status { get; set; } = status;

    /// <summary>Starts a headcount forecast in status created.</summary>
    public static HeadcountForecast NewHeadcountForecast(string code, string name)
    {
        return new HeadcountForecast(
            code,
            name,
            0,
            "",
            0.0,
            0,
            0.0,
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            HeadcountForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountForecastStatus next)
    {
        switch (Status)
        {
            case HeadcountForecastStatus.Created:
                return next == HeadcountForecastStatus.Retired;
            case HeadcountForecastStatus.Validated:
                return next == HeadcountForecastStatus.Published;
            case HeadcountForecastStatus.Published:
                return next == HeadcountForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(HeadcountForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double EffectiveLoad()
    {
        return Math.Round(Fte * (double)WindowDays, 2);
    }

    public HeadcountForecast Copy()
    {
        return new HeadcountForecast(
            Code,
            Name,
            WindowDays,
            Reviewer,
            Fte,
            StartsAt,
            Margin,
            Cost,
            SalaryBand,
            Minimum,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
