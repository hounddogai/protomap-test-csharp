namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty forecast calculation.</summary>
public interface IWarrantyForecastFactor
{
    string Describe();
    double Adjust(WarrantyForecast item, double valueValue);
}

public sealed record WarrantyForecastRetriesOffsetAdjuster(double Weight) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TitleWarrantyForecastAdjuster(Dictionary<string, double> Factors) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWarrantyForecastAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWarrantyForecastAdjuster(double Step) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWarrantyForecastAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
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

public sealed record CapWarrantyForecastAdjuster(double Cap) : IWarrantyForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the warranty forecast steps in order and keeps a trace of each.</summary>
public sealed class WarrantyForecastEngine(List<IWarrantyForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyForecastEngine DefaultWarrantyForecastEngine(Registry flags)
    {
        return new WarrantyForecastEngine(
            new List<IWarrantyForecastFactor>
            {
                new WarrantyForecastRetriesOffsetAdjuster(1.57),
                new TitleWarrantyForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.96, ["online"] = 0.88 }),
                new FlaggedWarrantyForecastAdjuster(flags, "warranty-forecast-lenient", -9.8),
                new RoundingWarrantyForecastAdjuster(5.0),
                new TieredWarrantyForecastAdjuster(
                    new List<double> { 2809.0, 8611.0 },
                    new List<double> { 0.98, 1.05 }),
                new CapWarrantyForecastAdjuster(11871.0),
            },
            new List<string>(),
            0.2,
            62888.8);
    }

    public double Evaluate(WarrantyForecast item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
