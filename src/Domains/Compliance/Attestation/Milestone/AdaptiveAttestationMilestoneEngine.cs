namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive attestation milestone calculation.</summary>
public interface IAdaptiveAttestationMilestoneModifier
{
    string Describe();
    double Adjust(AdaptiveAttestationMilestone item, double valueValue);
}

public sealed record RoundingAdaptiveAttestationMilestoneAdjuster(double Step) : IAdaptiveAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveAttestationMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveAttestationMilestoneAdjuster(double Percent) : IAdaptiveAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveAttestationMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionAdaptiveAttestationMilestoneAdjuster(Dictionary<string, double> Factors) : IAdaptiveAttestationMilestoneModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AdaptiveAttestationMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive attestation milestone steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAttestationMilestoneEngine(List<IAdaptiveAttestationMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAttestationMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAttestationMilestoneEngine DefaultAdaptiveAttestationMilestoneEngine()
    {
        return new AdaptiveAttestationMilestoneEngine(
            new List<IAdaptiveAttestationMilestoneModifier>
            {
                new RoundingAdaptiveAttestationMilestoneAdjuster(1.0),
                new PercentAdaptiveAttestationMilestoneAdjuster(-3.4),
                new RegionAdaptiveAttestationMilestoneAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.26, ["west"] = 1.0, ["online"] = 1.29 }),
            },
            new List<string>(),
            2.5,
            2212.0);
    }

    public double Compute(AdaptiveAttestationMilestone item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAttestationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
