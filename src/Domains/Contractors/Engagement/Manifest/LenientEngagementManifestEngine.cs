namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient engagement manifest calculation.</summary>
public interface ILenientEngagementManifestModifier
{
    string Describe();
    double Adjust(LenientEngagementManifest item, double valueValue);
}

public sealed record PercentLenientEngagementManifestAdjuster(double Percent) : ILenientEngagementManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientEngagementManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientEngagementManifestAdjuster(double Minimum) : ILenientEngagementManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientEngagementManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLenientEngagementManifestAdjuster(double Step) : ILenientEngagementManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientEngagementManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient engagement manifest steps in order and keeps a trace of each.</summary>
public sealed class LenientEngagementManifestEngine(List<ILenientEngagementManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientEngagementManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientEngagementManifestEngine DefaultLenientEngagementManifestEngine()
    {
        return new LenientEngagementManifestEngine(
            new List<ILenientEngagementManifestModifier>
            {
                new PercentLenientEngagementManifestAdjuster(9.5),
                new MinimumLenientEngagementManifestAdjuster(7.2),
                new RoundingLenientEngagementManifestAdjuster(0.5),
            },
            new List<string>(),
            2.7,
            88841.9);
    }

    public double Compute(LenientEngagementManifest item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientEngagementManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
