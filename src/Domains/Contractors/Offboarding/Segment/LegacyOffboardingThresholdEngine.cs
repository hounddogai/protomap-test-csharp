namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy offboarding threshold calculation.</summary>
public interface ILegacyOffboardingThresholdFactor
{
    string Describe();
    double Adjust(LegacyOffboardingThreshold item, double valueValue);
}

public sealed record RoundingLegacyOffboardingThresholdAdjuster(double Step) : ILegacyOffboardingThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyOffboardingThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyOffboardingThresholdAdjuster(List<double> Bounds, List<double> Rates) : ILegacyOffboardingThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyOffboardingThreshold item, double valueValue)
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

public sealed record TitleLegacyOffboardingThresholdAdjuster(Dictionary<string, double> Factors) : ILegacyOffboardingThresholdFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyOffboardingThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy offboarding threshold steps in order and keeps a trace of each.</summary>
public sealed class LegacyOffboardingThresholdEngine(List<ILegacyOffboardingThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOffboardingThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOffboardingThresholdEngine DefaultLegacyOffboardingThresholdEngine()
    {
        return new LegacyOffboardingThresholdEngine(
            new List<ILegacyOffboardingThresholdFactor>
            {
                new RoundingLegacyOffboardingThresholdAdjuster(0.5),
                new TieredLegacyOffboardingThresholdAdjuster(
                    new List<double> { 5099.0, 5783.0 },
                    new List<double> { 1.14, 0.98 }),
                new TitleLegacyOffboardingThresholdAdjuster(new Dictionary<string, double> { ["central"] = 1.08 }),
            },
            new List<string>(),
            3.6,
            1847.1);
    }

    public double Compute(LegacyOffboardingThreshold item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOffboardingThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
