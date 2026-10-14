namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall exception calculation.</summary>
public interface ITownHallExceptionFactor
{
    string Describe();
    double Adjust(TownHallException item, double valueValue);
}

public sealed record TownHallExceptionVersionOffsetAdjuster(double Weight) : ITownHallExceptionFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TownHallException item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record FlaggedTownHallExceptionAdjuster(Registry Flags, string Flag, double Percent) : ITownHallExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTownHallExceptionAdjuster(double Step) : ITownHallExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTownHallExceptionAdjuster(double Cap) : ITownHallExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTownHallExceptionAdjuster(List<double> Bounds, List<double> Rates) : ITownHallExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallException item, double valueValue)
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

/// <summary>Runs the town hall exception steps in order and keeps a trace of each.</summary>
public sealed class TownHallExceptionEngine(List<ITownHallExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallExceptionEngine DefaultTownHallExceptionEngine(Registry flags)
    {
        return new TownHallExceptionEngine(
            new List<ITownHallExceptionFactor>
            {
                new TownHallExceptionVersionOffsetAdjuster(0.98),
                new FlaggedTownHallExceptionAdjuster(flags, "town-hall-exception-seasonal", 5.9),
                new RoundingTownHallExceptionAdjuster(0.05),
                new CapTownHallExceptionAdjuster(25972.0),
                new TieredTownHallExceptionAdjuster(
                    new List<double> { 2889.0, 6396.0 },
                    new List<double> { 1.11, 0.92 }),
            },
            new List<string>(),
            2.7,
            28164.8);
    }

    public double Run(TownHallException item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
