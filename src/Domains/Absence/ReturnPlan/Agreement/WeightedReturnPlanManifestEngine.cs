namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted return plan manifest calculation.</summary>
public interface IWeightedReturnPlanManifestModifier
{
    string Describe();
    double Adjust(WeightedReturnPlanManifest item, double valueValue);
}

public sealed record TieredWeightedReturnPlanManifestAdjuster(List<double> Bounds, List<double> Rates) : IWeightedReturnPlanManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedReturnPlanManifest item, double valueValue)
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

public sealed record FlaggedWeightedReturnPlanManifestAdjuster(Registry Flags, string Flag, double Percent) : IWeightedReturnPlanManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedReturnPlanManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedReturnPlanManifestAdjuster(double Percent) : IWeightedReturnPlanManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedReturnPlanManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedReturnPlanManifestAdjuster(double Step) : IWeightedReturnPlanManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedReturnPlanManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedReturnPlanManifestMaximumOffsetAdjuster(double Weight) : IWeightedReturnPlanManifestModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(WeightedReturnPlanManifest item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the weighted return plan manifest steps in order and keeps a trace of each.</summary>
public sealed class WeightedReturnPlanManifestEngine(List<IWeightedReturnPlanManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedReturnPlanManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedReturnPlanManifestEngine DefaultWeightedReturnPlanManifestEngine(Registry flags)
    {
        return new WeightedReturnPlanManifestEngine(
            new List<IWeightedReturnPlanManifestModifier>
            {
                new TieredWeightedReturnPlanManifestAdjuster(
                    new List<double> { 649.0, 1928.0, 2250.0, 2339.0 },
                    new List<double> { 1.13, 0.93, 0.91, 0.86 }),
                new FlaggedWeightedReturnPlanManifestAdjuster(flags, "weighted-return-plan-manifest-adaptive", 6.5),
                new PercentWeightedReturnPlanManifestAdjuster(-3.4),
                new RoundingWeightedReturnPlanManifestAdjuster(0.25),
                new WeightedReturnPlanManifestMaximumOffsetAdjuster(0.07),
            },
            new List<string>(),
            2.1,
            14639.6);
    }

    public double Compute(WeightedReturnPlanManifest item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedReturnPlanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
