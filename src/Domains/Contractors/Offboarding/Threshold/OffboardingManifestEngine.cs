namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding manifest calculation.</summary>
public interface IOffboardingManifestAdjuster
{
    string Describe();
    double Adjust(OffboardingManifest item, double valueValue);
}

public sealed record OffboardingManifestExpiresAtOffsetAdjuster(double Weight) : IOffboardingManifestAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(OffboardingManifest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingOffboardingManifestAdjuster(double Step) : IOffboardingManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentOffboardingManifestAdjuster(double Percent) : IOffboardingManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding manifest steps in order and keeps a trace of each.</summary>
public sealed class OffboardingManifestEngine(List<IOffboardingManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingManifestEngine DefaultOffboardingManifestEngine()
    {
        return new OffboardingManifestEngine(
            new List<IOffboardingManifestAdjuster>
            {
                new OffboardingManifestExpiresAtOffsetAdjuster(0.41),
                new RoundingOffboardingManifestAdjuster(5.0),
                new PercentOffboardingManifestAdjuster(20.2),
            },
            new List<string>(),
            1.5,
            50891.7);
    }

    public double Evaluate(OffboardingManifest item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
