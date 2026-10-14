namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model override calculation.</summary>
public interface ICostModelOverrideAdjuster
{
    string Describe();
    double Adjust(CostModelOverride item, double valueValue);
}

public sealed record CostModelOverrideFloorValueOffsetAdjuster(double Weight) : ICostModelOverrideAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CostModelOverride item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredCostModelOverrideAdjuster(List<double> Bounds, List<double> Rates) : ICostModelOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelOverride item, double valueValue)
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

public sealed record LocaleCostModelOverrideAdjuster(Dictionary<string, double> Factors) : ICostModelOverrideAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CostModelOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCostModelOverrideAdjuster(Registry Flags, string Flag, double Percent) : ICostModelOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCostModelOverrideAdjuster(double Step) : ICostModelOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the cost model override steps in order and keeps a trace of each.</summary>
public sealed class CostModelOverrideEngine(List<ICostModelOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelOverrideEngine DefaultCostModelOverrideEngine(Registry flags)
    {
        return new CostModelOverrideEngine(
            new List<ICostModelOverrideAdjuster>
            {
                new CostModelOverrideFloorValueOffsetAdjuster(0.83),
                new TieredCostModelOverrideAdjuster(
                    new List<double> { 3118.0, 5460.0 },
                    new List<double> { 1.03, 1.12 }),
                new LocaleCostModelOverrideAdjuster(new Dictionary<string, double> { ["retail"] = 1.01 }),
                new FlaggedCostModelOverrideAdjuster(flags, "cost-model-override-fallback", 5.3),
                new RoundingCostModelOverrideAdjuster(0.25),
            },
            new List<string>(),
            0.7,
            42237.9);
    }

    public double Compute(CostModelOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
