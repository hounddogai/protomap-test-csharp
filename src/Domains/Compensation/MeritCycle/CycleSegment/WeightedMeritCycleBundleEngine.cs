namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted merit cycle bundle calculation.</summary>
public interface IWeightedMeritCycleBundleAdjuster
{
    string Describe();
    double Adjust(WeightedMeritCycleBundle item, double valueValue);
}

public sealed record MinimumWeightedMeritCycleBundleAdjuster(double Minimum) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedMeritCycleBundleAdjuster(double Step) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWeightedMeritCycleBundleAdjuster(Registry Flags, string Flag, double Percent) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedMeritCycleBundleAdjuster(double Percent) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedMeritCycleBundleRetriesOffsetAdjuster(double Weight) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CountryWeightedMeritCycleBundleAdjuster(Dictionary<string, double> Factors) : IWeightedMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(WeightedMeritCycleBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted merit cycle bundle steps in order and keeps a trace of each.</summary>
public sealed class WeightedMeritCycleBundleEngine(List<IWeightedMeritCycleBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedMeritCycleBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedMeritCycleBundleEngine DefaultWeightedMeritCycleBundleEngine(Registry flags)
    {
        return new WeightedMeritCycleBundleEngine(
            new List<IWeightedMeritCycleBundleAdjuster>
            {
                new MinimumWeightedMeritCycleBundleAdjuster(12.4),
                new RoundingWeightedMeritCycleBundleAdjuster(0.25),
                new FlaggedWeightedMeritCycleBundleAdjuster(flags, "weighted-merit-cycle-bundle-strict", -3.9),
                new PercentWeightedMeritCycleBundleAdjuster(8.5),
                new WeightedMeritCycleBundleRetriesOffsetAdjuster(0.52),
                new CountryWeightedMeritCycleBundleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.74, ["north"] = 1.19 }),
            },
            new List<string>(),
            2.0,
            21054.1);
    }

    public double Evaluate(WeightedMeritCycleBundle item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedMeritCycleBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
