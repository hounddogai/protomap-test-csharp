namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall forecast calculation.</summary>
public interface ITownHallForecastFactor
{
    string Describe();
    double Adjust(TownHallForecast item, double valueValue);
}

public sealed record MinimumTownHallForecastAdjuster(double Minimum) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTownHallForecastAdjuster(double Percent) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTownHallForecastAdjuster(List<double> Bounds, List<double> Rates) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallForecast item, double valueValue)
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

public sealed record TownHallForecastBudgetOffsetAdjuster(double Weight) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(TownHallForecast item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingTownHallForecastAdjuster(double Step) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTownHallForecastAdjuster(Registry Flags, string Flag, double Percent) : ITownHallForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall forecast steps in order and keeps a trace of each.</summary>
public sealed class TownHallForecastEngine(List<ITownHallForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallForecastEngine DefaultTownHallForecastEngine(Registry flags)
    {
        return new TownHallForecastEngine(
            new List<ITownHallForecastFactor>
            {
                new MinimumTownHallForecastAdjuster(35.6),
                new PercentTownHallForecastAdjuster(-11.5),
                new TieredTownHallForecastAdjuster(
                    new List<double> { 1724.0, 4164.0, 7435.0, 9542.0 },
                    new List<double> { 0.83, 0.83, 0.81, 0.95 }),
                new TownHallForecastBudgetOffsetAdjuster(0.48),
                new RoundingTownHallForecastAdjuster(0.5),
                new FlaggedTownHallForecastAdjuster(flags, "town-hall-forecast-fallback", 1.3),
            },
            new List<string>(),
            4.2,
            82482.6);
    }

    public double Compute(TownHallForecast item)
    {
        var current = item.AdjustedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<TownHallForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
