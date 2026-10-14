namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback engagement profile calculation.</summary>
public interface IFallbackEngagementProfileModifier
{
    string Describe();
    double Adjust(FallbackEngagementProfile item, double valueValue);
}

public sealed record FlaggedFallbackEngagementProfileAdjuster(Registry Flags, string Flag, double Percent) : IFallbackEngagementProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackEngagementProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackEngagementProfileAdjuster(double Step) : IFallbackEngagementProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackEngagementProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackEngagementProfileAdjuster(double Minimum) : IFallbackEngagementProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackEngagementProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackEngagementProfileCostOffsetAdjuster(double Weight) : IFallbackEngagementProfileModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(FallbackEngagementProfile item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the fallback engagement profile steps in order and keeps a trace of each.</summary>
public sealed class FallbackEngagementProfileEngine(List<IFallbackEngagementProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackEngagementProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackEngagementProfileEngine DefaultFallbackEngagementProfileEngine(Registry flags)
    {
        return new FallbackEngagementProfileEngine(
            new List<IFallbackEngagementProfileModifier>
            {
                new FlaggedFallbackEngagementProfileAdjuster(flags, "fallback-engagement-profile-manual", 7.0),
                new RoundingFallbackEngagementProfileAdjuster(1.0),
                new MinimumFallbackEngagementProfileAdjuster(21.7),
                new FallbackEngagementProfileCostOffsetAdjuster(1.54),
            },
            new List<string>(),
            4.1,
            92830.6);
    }

    public double Evaluate(FallbackEngagementProfile item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackEngagementProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
