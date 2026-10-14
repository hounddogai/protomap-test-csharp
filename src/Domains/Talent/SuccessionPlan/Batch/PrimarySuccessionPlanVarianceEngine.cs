namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary succession plan variance calculation.</summary>
public interface IPrimarySuccessionPlanVarianceModifier
{
    string Describe();
    double Adjust(PrimarySuccessionPlanVariance item, double valueValue);
}

public sealed record TieredPrimarySuccessionPlanVarianceAdjuster(List<double> Bounds, List<double> Rates) : IPrimarySuccessionPlanVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimarySuccessionPlanVariance item, double valueValue)
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

public sealed record UnitPrimarySuccessionPlanVarianceAdjuster(Dictionary<string, double> Factors) : IPrimarySuccessionPlanVarianceModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PrimarySuccessionPlanVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimarySuccessionPlanVarianceAdjuster(double Step) : IPrimarySuccessionPlanVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySuccessionPlanVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary succession plan variance steps in order and keeps a trace of each.</summary>
public sealed class PrimarySuccessionPlanVarianceEngine(List<IPrimarySuccessionPlanVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySuccessionPlanVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySuccessionPlanVarianceEngine DefaultPrimarySuccessionPlanVarianceEngine()
    {
        return new PrimarySuccessionPlanVarianceEngine(
            new List<IPrimarySuccessionPlanVarianceModifier>
            {
                new TieredPrimarySuccessionPlanVarianceAdjuster(
                    new List<double> { 1167.0, 7586.0, 9547.0 },
                    new List<double> { 0.81, 0.86, 0.8 }),
                new UnitPrimarySuccessionPlanVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["retail"] = 0.94 }),
                new RoundingPrimarySuccessionPlanVarianceAdjuster(0.1),
            },
            new List<string>(),
            3.3,
            95473.6);
    }

    public double Evaluate(PrimarySuccessionPlanVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySuccessionPlanVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
