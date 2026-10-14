namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback cost model rollup calculation.</summary>
public interface IFallbackCostModelRollupStep
{
    string Describe();
    double Adjust(FallbackCostModelRollup item, double valueValue);
}

public sealed record FlaggedFallbackCostModelRollupAdjuster(Registry Flags, string Flag, double Percent) : IFallbackCostModelRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackCostModelRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceFallbackCostModelRollupAdjuster(Dictionary<string, double> Factors) : IFallbackCostModelRollupStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(FallbackCostModelRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFallbackCostModelRollupAdjuster(double Minimum) : IFallbackCostModelRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackCostModelRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFallbackCostModelRollupAdjuster(double Step) : IFallbackCostModelRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackCostModelRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback cost model rollup steps in order and keeps a trace of each.</summary>
public sealed class FallbackCostModelRollupEngine(List<IFallbackCostModelRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackCostModelRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackCostModelRollupEngine DefaultFallbackCostModelRollupEngine(Registry flags)
    {
        return new FallbackCostModelRollupEngine(
            new List<IFallbackCostModelRollupStep>
            {
                new FlaggedFallbackCostModelRollupAdjuster(flags, "fallback-cost-model-rollup-rolling", -2.8),
                new SourceFallbackCostModelRollupAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["central"] = 1.32, ["west"] = 1.28 }),
                new MinimumFallbackCostModelRollupAdjuster(15.6),
                new RoundingFallbackCostModelRollupAdjuster(5.0),
            },
            new List<string>(),
            0.7,
            18524.6);
    }

    public double Evaluate(FallbackCostModelRollup item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackCostModelRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
