namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered scenario draft calculation.</summary>
public interface ITieredScenarioDraftModifier
{
    string Describe();
    double Adjust(TieredScenarioDraft item, double valueValue);
}

public sealed record CapTieredScenarioDraftAdjuster(double Cap) : ITieredScenarioDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredScenarioDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredScenarioDraftAdjuster(double Percent) : ITieredScenarioDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredScenarioDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScenarioDraftSequenceOffsetAdjuster(double Weight) : ITieredScenarioDraftModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(TieredScenarioDraft item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedTieredScenarioDraftAdjuster(Registry Flags, string Flag, double Percent) : ITieredScenarioDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredScenarioDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredScenarioDraftAdjuster(double Step) : ITieredScenarioDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredScenarioDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered scenario draft steps in order and keeps a trace of each.</summary>
public sealed class TieredScenarioDraftEngine(List<ITieredScenarioDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredScenarioDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredScenarioDraftEngine DefaultTieredScenarioDraftEngine(Registry flags)
    {
        return new TieredScenarioDraftEngine(
            new List<ITieredScenarioDraftModifier>
            {
                new CapTieredScenarioDraftAdjuster(19955.0),
                new PercentTieredScenarioDraftAdjuster(-24.1),
                new TieredScenarioDraftSequenceOffsetAdjuster(1.62),
                new FlaggedTieredScenarioDraftAdjuster(flags, "tiered-scenario-draft-primary", -4.6),
                new RoundingTieredScenarioDraftAdjuster(0.25),
            },
            new List<string>(),
            0.9,
            21079.8);
    }

    public double Compute(TieredScenarioDraft item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredScenarioDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
