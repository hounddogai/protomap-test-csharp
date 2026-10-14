namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive policy quota calculation.</summary>
public interface IAdaptivePolicyQuotaAdjuster
{
    string Describe();
    double Adjust(AdaptivePolicyQuota item, double valueValue);
}

public sealed record TitleAdaptivePolicyQuotaAdjuster(Dictionary<string, double> Factors) : IAdaptivePolicyQuotaAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdaptivePolicyQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptivePolicyQuotaAdjuster(double Step) : IAdaptivePolicyQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePolicyQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptivePolicyQuotaAdjuster(double Percent) : IAdaptivePolicyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePolicyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptivePolicyQuotaAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptivePolicyQuotaAdjuster(double Cap) : IAdaptivePolicyQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePolicyQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive policy quota steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyQuotaEngine(List<IAdaptivePolicyQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyQuotaEngine DefaultAdaptivePolicyQuotaEngine(Registry flags)
    {
        return new AdaptivePolicyQuotaEngine(
            new List<IAdaptivePolicyQuotaAdjuster>
            {
                new TitleAdaptivePolicyQuotaAdjuster(new Dictionary<string, double> { ["north"] = 1.09 }),
                new RoundingAdaptivePolicyQuotaAdjuster(1.0),
                new PercentAdaptivePolicyQuotaAdjuster(-2.1),
                new FlaggedAdaptivePolicyQuotaAdjuster(flags, "adaptive-policy-quota-weighted", 0.5),
                new CapAdaptivePolicyQuotaAdjuster(44501.0),
            },
            new List<string>(),
            3.9,
            39498.3);
    }

    public double Run(AdaptivePolicyQuota item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
