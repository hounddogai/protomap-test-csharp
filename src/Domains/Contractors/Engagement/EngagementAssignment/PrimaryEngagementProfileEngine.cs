namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>One step of the primary engagement profile calculation.</summary>
public interface IPrimaryEngagementProfileStep
{
    string Describe();
    double Adjust(PrimaryEngagementProfile item, double valueValue);
}

public sealed record PercentPrimaryEngagementProfileAdjuster(double Percent) : IPrimaryEngagementProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryEngagementProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryEngagementProfileAdjuster(double Cap) : IPrimaryEngagementProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryEngagementProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourcePrimaryEngagementProfileAdjuster(Dictionary<string, double> Factors) : IPrimaryEngagementProfileStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PrimaryEngagementProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryEngagementProfileAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryEngagementProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryEngagementProfile item, double valueValue)
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

/// <summary>Runs the primary engagement profile steps in order and keeps a trace of each.</summary>
public sealed class PrimaryEngagementProfileEngine(List<IPrimaryEngagementProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryEngagementProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryEngagementProfileEngine DefaultPrimaryEngagementProfileEngine()
    {
        return new PrimaryEngagementProfileEngine(
            new List<IPrimaryEngagementProfileStep>
            {
                new PercentPrimaryEngagementProfileAdjuster(24.4),
                new CapPrimaryEngagementProfileAdjuster(27506.0),
                new SourcePrimaryEngagementProfileAdjuster(new Dictionary<string, double> { ["south"] = 1.04 }),
                new TieredPrimaryEngagementProfileAdjuster(
                    new List<double> { 5028.0, 8244.0 },
                    new List<double> { 1.0, 0.86 }),
            },
            new List<string>(),
            4.9,
            9981.2);
    }

    public double Price(PrimaryEngagementProfile item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryEngagementProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
