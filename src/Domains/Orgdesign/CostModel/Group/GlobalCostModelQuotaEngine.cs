namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global cost model quota calculation.</summary>
public interface IGlobalCostModelQuotaModifier
{
    string Describe();
    double Adjust(GlobalCostModelQuota item, double valueValue);
}

public sealed record NameGlobalCostModelQuotaAdjuster(Dictionary<string, double> Factors) : IGlobalCostModelQuotaModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalCostModelQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalCostModelQuotaAdjuster(double Minimum) : IGlobalCostModelQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalCostModelQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalCostModelQuotaAdjuster(List<double> Bounds, List<double> Rates) : IGlobalCostModelQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalCostModelQuota item, double valueValue)
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

public sealed record GlobalCostModelQuotaRateOffsetAdjuster(double Weight) : IGlobalCostModelQuotaModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(GlobalCostModelQuota item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedGlobalCostModelQuotaAdjuster(Registry Flags, string Flag, double Percent) : IGlobalCostModelQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalCostModelQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global cost model quota steps in order and keeps a trace of each.</summary>
public sealed class GlobalCostModelQuotaEngine(List<IGlobalCostModelQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalCostModelQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalCostModelQuotaEngine DefaultGlobalCostModelQuotaEngine(Registry flags)
    {
        return new GlobalCostModelQuotaEngine(
            new List<IGlobalCostModelQuotaModifier>
            {
                new NameGlobalCostModelQuotaAdjuster(new Dictionary<string, double> { ["east"] = 1.21 }),
                new MinimumGlobalCostModelQuotaAdjuster(3.9),
                new TieredGlobalCostModelQuotaAdjuster(
                    new List<double> { 1045.0, 2755.0, 4039.0, 9791.0 },
                    new List<double> { 0.98, 1.04, 1.06, 0.88 }),
                new GlobalCostModelQuotaRateOffsetAdjuster(0.42),
                new FlaggedGlobalCostModelQuotaAdjuster(flags, "global-cost-model-quota-seasonal", -6.4),
            },
            new List<string>(),
            1.4,
            28737.2);
    }

    public double Run(GlobalCostModelQuota item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalCostModelQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
