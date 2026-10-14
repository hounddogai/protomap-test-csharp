namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model policy calculation.</summary>
public interface ICostModelPolicyFactor
{
    string Describe();
    double Adjust(CostModelPolicy item, double valueValue);
}

public sealed record CostModelPolicyThresholdOffsetAdjuster(double Weight) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record PercentCostModelPolicyAdjuster(double Percent) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCostModelPolicyAdjuster(double Step) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCostModelPolicyAdjuster(List<double> Bounds, List<double> Rates) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
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

public sealed record FlaggedCostModelPolicyAdjuster(Registry Flags, string Flag, double Percent) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelPolicyAdjuster(double Minimum) : ICostModelPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the cost model policy steps in order and keeps a trace of each.</summary>
public sealed class CostModelPolicyEngine(List<ICostModelPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelPolicyEngine DefaultCostModelPolicyEngine(Registry flags)
    {
        return new CostModelPolicyEngine(
            new List<ICostModelPolicyFactor>
            {
                new CostModelPolicyThresholdOffsetAdjuster(1.59),
                new PercentCostModelPolicyAdjuster(-19.9),
                new RoundingCostModelPolicyAdjuster(1.0),
                new TieredCostModelPolicyAdjuster(
                    new List<double> { 267.0, 9326.0, 9751.0 },
                    new List<double> { 1.06, 1.06, 0.82 }),
                new FlaggedCostModelPolicyAdjuster(flags, "cost-model-policy-regional", 0.3),
                new MinimumCostModelPolicyAdjuster(19.3),
            },
            new List<string>(),
            1.3,
            5668.3);
    }

    public double Price(CostModelPolicy item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
