namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict rate forecast calculation.</summary>
public interface IStrictRateForecastFactor
{
    string Describe();
    double Adjust(StrictRateForecast item, double valueValue);
}

public sealed record FlaggedStrictRateForecastAdjuster(Registry Flags, string Flag, double Percent) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitStrictRateForecastAdjuster(Dictionary<string, double> Factors) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictRateForecastAdjuster(double Percent) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictRateForecastHoursOffsetAdjuster(double Weight) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingStrictRateForecastAdjuster(double Step) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStrictRateForecastAdjuster(List<double> Bounds, List<double> Rates) : IStrictRateForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictRateForecast item, double valueValue)
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

/// <summary>Runs the strict rate forecast steps in order and keeps a trace of each.</summary>
public sealed class StrictRateForecastEngine(List<IStrictRateForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictRateForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictRateForecastEngine DefaultStrictRateForecastEngine(Registry flags)
    {
        return new StrictRateForecastEngine(
            new List<IStrictRateForecastFactor>
            {
                new FlaggedStrictRateForecastAdjuster(flags, "strict-rate-forecast-tiered", 1.3),
                new UnitStrictRateForecastAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.12, ["south"] = 1.35 }),
                new PercentStrictRateForecastAdjuster(20.9),
                new StrictRateForecastHoursOffsetAdjuster(0.43),
                new RoundingStrictRateForecastAdjuster(1.0),
                new TieredStrictRateForecastAdjuster(
                    new List<double> { 1357.0, 7632.0 },
                    new List<double> { 1.07, 0.83 }),
            },
            new List<string>(),
            0.1,
            51102.4);
    }

    public double Compute(StrictRateForecast item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictRateForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
