namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback cost model group calculation.</summary>
public interface IFallbackCostModelGroupAdjuster
{
    string Describe();
    double Adjust(FallbackCostModelGroup item, double valueValue);
}

public sealed record TieredFallbackCostModelGroupAdjuster(List<double> Bounds, List<double> Rates) : IFallbackCostModelGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackCostModelGroup item, double valueValue)
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

public sealed record RoundingFallbackCostModelGroupAdjuster(double Step) : IFallbackCostModelGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackCostModelGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackCostModelGroupAdjuster(double Percent) : IFallbackCostModelGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackCostModelGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback cost model group steps in order and keeps a trace of each.</summary>
public sealed class FallbackCostModelGroupEngine(List<IFallbackCostModelGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackCostModelGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackCostModelGroupEngine DefaultFallbackCostModelGroupEngine()
    {
        return new FallbackCostModelGroupEngine(
            new List<IFallbackCostModelGroupAdjuster>
            {
                new TieredFallbackCostModelGroupAdjuster(
                    new List<double> { 6401.0, 8262.0, 8887.0, 9378.0 },
                    new List<double> { 0.9, 0.83, 1.03, 0.88 }),
                new RoundingFallbackCostModelGroupAdjuster(0.05),
                new PercentFallbackCostModelGroupAdjuster(22.4),
            },
            new List<string>(),
            4.2,
            56714.2);
    }

    public double Evaluate(FallbackCostModelGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackCostModelGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
