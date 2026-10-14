namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted policy override calculation.</summary>
public interface IWeightedPolicyOverrideModifier
{
    string Describe();
    double Adjust(WeightedPolicyOverride item, double valueValue);
}

public sealed record TieredWeightedPolicyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPolicyOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPolicyOverride item, double valueValue)
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

public sealed record RoundingWeightedPolicyOverrideAdjuster(double Step) : IWeightedPolicyOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPolicyOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameWeightedPolicyOverrideAdjuster(Dictionary<string, double> Factors) : IWeightedPolicyOverrideModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedPolicyOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted policy override steps in order and keeps a trace of each.</summary>
public sealed class WeightedPolicyOverrideEngine(List<IWeightedPolicyOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPolicyOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPolicyOverrideEngine DefaultWeightedPolicyOverrideEngine()
    {
        return new WeightedPolicyOverrideEngine(
            new List<IWeightedPolicyOverrideModifier>
            {
                new TieredWeightedPolicyOverrideAdjuster(
                    new List<double> { 2287.0, 2347.0 },
                    new List<double> { 1.16, 0.82 }),
                new RoundingWeightedPolicyOverrideAdjuster(0.25),
                new NameWeightedPolicyOverrideAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["retail"] = 1.03, ["central"] = 0.94 }),
            },
            new List<string>(),
            4.0,
            99952.0);
    }

    public double Compute(WeightedPolicyOverride item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPolicyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
