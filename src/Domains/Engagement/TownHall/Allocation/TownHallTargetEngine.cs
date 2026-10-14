namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall target calculation.</summary>
public interface ITownHallTargetModifier
{
    string Describe();
    double Adjust(TownHallTarget item, double valueValue);
}

public sealed record TownHallTargetQuantityOffsetAdjuster(double Weight) : ITownHallTargetModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(TownHallTarget item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedTownHallTargetAdjuster(Registry Flags, string Flag, double Percent) : ITownHallTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTownHallTargetAdjuster(List<double> Bounds, List<double> Rates) : ITownHallTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallTarget item, double valueValue)
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

public sealed record RoundingTownHallTargetAdjuster(double Step) : ITownHallTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the town hall target steps in order and keeps a trace of each.</summary>
public sealed class TownHallTargetEngine(List<ITownHallTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallTargetEngine DefaultTownHallTargetEngine(Registry flags)
    {
        return new TownHallTargetEngine(
            new List<ITownHallTargetModifier>
            {
                new TownHallTargetQuantityOffsetAdjuster(1.38),
                new FlaggedTownHallTargetAdjuster(flags, "town-hall-target-fallback", -0.3),
                new TieredTownHallTargetAdjuster(
                    new List<double> { 65.0, 857.0, 6597.0 },
                    new List<double> { 0.81, 0.92, 1.07 }),
                new RoundingTownHallTargetAdjuster(5.0),
            },
            new List<string>(),
            1.6,
            92658.6);
    }

    public double Price(TownHallTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
