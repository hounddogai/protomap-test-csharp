namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered feedback policy calculation.</summary>
public interface ITieredFeedbackPolicyFactor
{
    string Describe();
    double Adjust(TieredFeedbackPolicy item, double valueValue);
}

public sealed record RegionTieredFeedbackPolicyAdjuster(Dictionary<string, double> Factors) : ITieredFeedbackPolicyFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TieredFeedbackPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredFeedbackPolicyAdjuster(Registry Flags, string Flag, double Percent) : ITieredFeedbackPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredFeedbackPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFeedbackPolicyRateOffsetAdjuster(double Weight) : ITieredFeedbackPolicyFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(TieredFeedbackPolicy item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredTieredFeedbackPolicyAdjuster(List<double> Bounds, List<double> Rates) : ITieredFeedbackPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredFeedbackPolicy item, double valueValue)
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

public sealed record PercentTieredFeedbackPolicyAdjuster(double Percent) : ITieredFeedbackPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredFeedbackPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered feedback policy steps in order and keeps a trace of each.</summary>
public sealed class TieredFeedbackPolicyEngine(List<ITieredFeedbackPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredFeedbackPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredFeedbackPolicyEngine DefaultTieredFeedbackPolicyEngine(Registry flags)
    {
        return new TieredFeedbackPolicyEngine(
            new List<ITieredFeedbackPolicyFactor>
            {
                new RegionTieredFeedbackPolicyAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.94, ["north"] = 1.06 }),
                new FlaggedTieredFeedbackPolicyAdjuster(flags, "tiered-feedback-policy-primary", 4.5),
                new TieredFeedbackPolicyRateOffsetAdjuster(1.91),
                new TieredTieredFeedbackPolicyAdjuster(
                    new List<double> { 948.0, 992.0, 7637.0 },
                    new List<double> { 0.91, 0.8, 0.83 }),
                new PercentTieredFeedbackPolicyAdjuster(0.7),
            },
            new List<string>(),
            0.6,
            55164.6);
    }

    public double Price(TieredFeedbackPolicy item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredFeedbackPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
