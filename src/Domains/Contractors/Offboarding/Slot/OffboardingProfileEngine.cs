namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding profile calculation.</summary>
public interface IOffboardingProfileAdjuster
{
    string Describe();
    double Adjust(OffboardingProfile item, double valueValue);
}

public sealed record RoundingOffboardingProfileAdjuster(double Step) : IOffboardingProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredOffboardingProfileAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingProfile item, double valueValue)
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

public sealed record FlaggedOffboardingProfileAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentOffboardingProfileAdjuster(double Percent) : IOffboardingProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding profile steps in order and keeps a trace of each.</summary>
public sealed class OffboardingProfileEngine(List<IOffboardingProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingProfileEngine DefaultOffboardingProfileEngine(Registry flags)
    {
        return new OffboardingProfileEngine(
            new List<IOffboardingProfileAdjuster>
            {
                new RoundingOffboardingProfileAdjuster(0.05),
                new TieredOffboardingProfileAdjuster(
                    new List<double> { 2285.0, 3071.0, 3431.0 },
                    new List<double> { 0.93, 1.2, 0.92 }),
                new FlaggedOffboardingProfileAdjuster(flags, "offboarding-profile-weighted", 7.5),
                new PercentOffboardingProfileAdjuster(-17.4),
            },
            new List<string>(),
            0.3,
            52734.5);
    }

    public double Evaluate(OffboardingProfile item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
