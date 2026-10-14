namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive policy bundle calculation.</summary>
public interface IAdaptivePolicyBundleFactor
{
    string Describe();
    double Adjust(AdaptivePolicyBundle item, double valueValue);
}

public sealed record CapAdaptivePolicyBundleAdjuster(double Cap) : IAdaptivePolicyBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePolicyBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptivePolicyBundleAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePolicyBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePolicyBundle item, double valueValue)
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

public sealed record FlaggedAdaptivePolicyBundleAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePolicyBundleAdjuster(double Minimum) : IAdaptivePolicyBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePolicyBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleAdaptivePolicyBundleAdjuster(Dictionary<string, double> Factors) : IAdaptivePolicyBundleFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AdaptivePolicyBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive policy bundle steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyBundleEngine(List<IAdaptivePolicyBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyBundleEngine DefaultAdaptivePolicyBundleEngine(Registry flags)
    {
        return new AdaptivePolicyBundleEngine(
            new List<IAdaptivePolicyBundleFactor>
            {
                new CapAdaptivePolicyBundleAdjuster(7185.0),
                new TieredAdaptivePolicyBundleAdjuster(
                    new List<double> { 2967.0, 5712.0, 8595.0 },
                    new List<double> { 1.03, 0.85, 1.02 }),
                new FlaggedAdaptivePolicyBundleAdjuster(flags, "adaptive-policy-bundle-rolling", 4.4),
                new MinimumAdaptivePolicyBundleAdjuster(0.8),
                new LocaleAdaptivePolicyBundleAdjuster(new Dictionary<string, double> { ["south"] = 1.09 }),
            },
            new List<string>(),
            4.1,
            99632.2);
    }

    public double Evaluate(AdaptivePolicyBundle item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
