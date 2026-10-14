namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding claim calculation.</summary>
public interface IOffboardingClaimAdjuster
{
    string Describe();
    double Adjust(OffboardingClaim item, double valueValue);
}

public sealed record OffboardingClaimScoreOffsetAdjuster(double Weight) : IOffboardingClaimAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(OffboardingClaim item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedOffboardingClaimAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingClaimAdjuster(double Minimum) : IOffboardingClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingOffboardingClaimAdjuster(double Step) : IOffboardingClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameOffboardingClaimAdjuster(Dictionary<string, double> Factors) : IOffboardingClaimAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OffboardingClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the offboarding claim steps in order and keeps a trace of each.</summary>
public sealed class OffboardingClaimEngine(List<IOffboardingClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingClaimEngine DefaultOffboardingClaimEngine(Registry flags)
    {
        return new OffboardingClaimEngine(
            new List<IOffboardingClaimAdjuster>
            {
                new OffboardingClaimScoreOffsetAdjuster(1.53),
                new FlaggedOffboardingClaimAdjuster(flags, "offboarding-claim-global", -2.4),
                new MinimumOffboardingClaimAdjuster(4.8),
                new RoundingOffboardingClaimAdjuster(0.25),
                new NameOffboardingClaimAdjuster(new Dictionary<string, double> { ["east"] = 0.97 }),
            },
            new List<string>(),
            0.1,
            95786.3);
    }

    public double Run(OffboardingClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
