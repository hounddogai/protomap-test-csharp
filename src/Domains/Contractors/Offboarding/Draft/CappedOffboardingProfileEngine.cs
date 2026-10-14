namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped offboarding profile calculation.</summary>
public interface ICappedOffboardingProfileFactor
{
    string Describe();
    double Adjust(CappedOffboardingProfile item, double valueValue);
}

public sealed record CappedOffboardingProfileEndsAtOffsetAdjuster(double Weight) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapCappedOffboardingProfileAdjuster(double Cap) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedOffboardingProfileAdjuster(double Step) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedOffboardingProfileAdjuster(Registry Flags, string Flag, double Percent) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedOffboardingProfileAdjuster(double Percent) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedOffboardingProfileAdjuster(List<double> Bounds, List<double> Rates) : ICappedOffboardingProfileFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedOffboardingProfile item, double valueValue)
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

/// <summary>Runs the capped offboarding profile steps in order and keeps a trace of each.</summary>
public sealed class CappedOffboardingProfileEngine(List<ICappedOffboardingProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedOffboardingProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedOffboardingProfileEngine DefaultCappedOffboardingProfileEngine(Registry flags)
    {
        return new CappedOffboardingProfileEngine(
            new List<ICappedOffboardingProfileFactor>
            {
                new CappedOffboardingProfileEndsAtOffsetAdjuster(1.88),
                new CapCappedOffboardingProfileAdjuster(33005.0),
                new RoundingCappedOffboardingProfileAdjuster(0.05),
                new FlaggedCappedOffboardingProfileAdjuster(flags, "capped-offboarding-profile-default", 1.0),
                new PercentCappedOffboardingProfileAdjuster(-20.3),
                new TieredCappedOffboardingProfileAdjuster(
                    new List<double> { 1367.0, 3309.0, 5768.0, 7424.0 },
                    new List<double> { 0.88, 0.95, 0.86, 0.91 }),
            },
            new List<string>(),
            2.8,
            16990.7);
    }

    public double Evaluate(CappedOffboardingProfile item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedOffboardingProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
