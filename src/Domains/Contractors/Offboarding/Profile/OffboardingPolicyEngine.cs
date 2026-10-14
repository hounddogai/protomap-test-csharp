namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding policy calculation.</summary>
public interface IOffboardingPolicyAdjuster
{
    string Describe();
    double Adjust(OffboardingPolicy item, double valueValue);
}

public sealed record TieredOffboardingPolicyAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
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

public sealed record CapOffboardingPolicyAdjuster(double Cap) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedOffboardingPolicyAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record OffboardingPolicyMaximumOffsetAdjuster(double Weight) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TeamOffboardingPolicyAdjuster(Dictionary<string, double> Factors) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingOffboardingPolicyAdjuster(double Step) : IOffboardingPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the offboarding policy steps in order and keeps a trace of each.</summary>
public sealed class OffboardingPolicyEngine(List<IOffboardingPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingPolicyEngine DefaultOffboardingPolicyEngine(Registry flags)
    {
        return new OffboardingPolicyEngine(
            new List<IOffboardingPolicyAdjuster>
            {
                new TieredOffboardingPolicyAdjuster(
                    new List<double> { 6431.0, 8366.0, 8897.0 },
                    new List<double> { 1.1, 0.99, 0.99 }),
                new CapOffboardingPolicyAdjuster(47575.0),
                new FlaggedOffboardingPolicyAdjuster(flags, "offboarding-policy-regional", -1.1),
                new OffboardingPolicyMaximumOffsetAdjuster(0.23),
                new TeamOffboardingPolicyAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["north"] = 0.94, ["south"] = 0.97 }),
                new RoundingOffboardingPolicyAdjuster(0.5),
            },
            new List<string>(),
            3.2,
            90872.3);
    }

    public double Run(OffboardingPolicy item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
