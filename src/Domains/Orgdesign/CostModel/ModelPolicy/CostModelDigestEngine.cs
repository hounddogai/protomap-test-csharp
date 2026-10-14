namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the cost model digest calculation.</summary>
public interface ICostModelDigestFactor
{
    string Describe();
    double Adjust(CostModelDigest item, double valueValue);
}

public sealed record LabelCostModelDigestAdjuster(Dictionary<string, double> Factors) : ICostModelDigestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CostModelDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCostModelDigestAdjuster(Registry Flags, string Flag, double Percent) : ICostModelDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelDigestAdjuster(List<double> Bounds, List<double> Rates) : ICostModelDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelDigest item, double valueValue)
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

/// <summary>Runs the cost model digest steps in order and keeps a trace of each.</summary>
public sealed class CostModelDigestEngine(List<ICostModelDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelDigestEngine DefaultCostModelDigestEngine(Registry flags)
    {
        return new CostModelDigestEngine(
            new List<ICostModelDigestFactor>
            {
                new LabelCostModelDigestAdjuster(new Dictionary<string, double> { ["east"] = 0.7, ["south"] = 0.83 }),
                new FlaggedCostModelDigestAdjuster(flags, "cost-model-digest-pending", 6.0),
                new TieredCostModelDigestAdjuster(
                    new List<double> { 987.0, 6062.0, 9601.0 },
                    new List<double> { 1.06, 1.01, 0.91 }),
            },
            new List<string>(),
            2.7,
            76345.7);
    }

    public double Evaluate(CostModelDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
