namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped rate policy calculation.</summary>
public interface ICappedRatePolicyStep
{
    string Describe();
    double Adjust(CappedRatePolicy item, double valueValue);
}

public sealed record TitleCappedRatePolicyAdjuster(Dictionary<string, double> Factors) : ICappedRatePolicyStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(CappedRatePolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCappedRatePolicyAdjuster(Registry Flags, string Flag, double Percent) : ICappedRatePolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRatePolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedRatePolicyAdjuster(double Cap) : ICappedRatePolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedRatePolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped rate policy steps in order and keeps a trace of each.</summary>
public sealed class CappedRatePolicyEngine(List<ICappedRatePolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRatePolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRatePolicyEngine DefaultCappedRatePolicyEngine(Registry flags)
    {
        return new CappedRatePolicyEngine(
            new List<ICappedRatePolicyStep>
            {
                new TitleCappedRatePolicyAdjuster(new Dictionary<string, double> { ["online"] = 1.29 }),
                new FlaggedCappedRatePolicyAdjuster(flags, "capped-rate-policy-legacy", -0.1),
                new CapCappedRatePolicyAdjuster(748.0),
            },
            new List<string>(),
            0.4,
            98546.6);
    }

    public double Evaluate(CappedRatePolicy item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRatePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
