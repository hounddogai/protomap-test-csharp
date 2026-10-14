namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal pay band template calculation.</summary>
public interface ISeasonalPayBandTemplateStep
{
    string Describe();
    double Adjust(SeasonalPayBandTemplate item, double valueValue);
}

public sealed record PercentSeasonalPayBandTemplateAdjuster(double Percent) : ISeasonalPayBandTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPayBandTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSeasonalPayBandTemplateAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPayBandTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPayBandTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameSeasonalPayBandTemplateAdjuster(Dictionary<string, double> Factors) : ISeasonalPayBandTemplateStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalPayBandTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalPayBandTemplateAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalPayBandTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalPayBandTemplate item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record CapSeasonalPayBandTemplateAdjuster(double Cap) : ISeasonalPayBandTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalPayBandTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal pay band template steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayBandTemplateEngine(List<ISeasonalPayBandTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayBandTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayBandTemplateEngine DefaultSeasonalPayBandTemplateEngine(Registry flags)
    {
        return new SeasonalPayBandTemplateEngine(
            new List<ISeasonalPayBandTemplateStep>
            {
                new PercentSeasonalPayBandTemplateAdjuster(-16.7),
                new FlaggedSeasonalPayBandTemplateAdjuster(flags, "seasonal-pay-band-template-legacy", 9.6),
                new NameSeasonalPayBandTemplateAdjuster(new Dictionary<string, double> { ["north"] = 0.76 }),
                new TieredSeasonalPayBandTemplateAdjuster(
                    new List<double> { 4200.0, 5362.0, 9388.0 },
                    new List<double> { 0.98, 0.8, 1.18 }),
                new CapSeasonalPayBandTemplateAdjuster(6784.0),
            },
            new List<string>(),
            3.5,
            88440.1);
    }

    public double Compute(SeasonalPayBandTemplate item)
    {
        var current = item.WeightedVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayBandTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
