namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding group calculation.</summary>
public interface IOffboardingGroupAdjuster
{
    string Describe();
    double Adjust(OffboardingGroup item, double valueValue);
}

public sealed record FlaggedOffboardingGroupAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingGroupAdjuster(double Minimum) : IOffboardingGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredOffboardingGroupAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingGroup item, double valueValue)
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

/// <summary>Runs the offboarding group steps in order and keeps a trace of each.</summary>
public sealed class OffboardingGroupEngine(List<IOffboardingGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingGroupEngine DefaultOffboardingGroupEngine(Registry flags)
    {
        return new OffboardingGroupEngine(
            new List<IOffboardingGroupAdjuster>
            {
                new FlaggedOffboardingGroupAdjuster(flags, "offboarding-group-pending", 5.5),
                new MinimumOffboardingGroupAdjuster(40.2),
                new TieredOffboardingGroupAdjuster(
                    new List<double> { 5950.0, 7122.0 },
                    new List<double> { 0.82, 1.19 }),
            },
            new List<string>(),
            3.4,
            59389.5);
    }

    public double Evaluate(OffboardingGroup item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
