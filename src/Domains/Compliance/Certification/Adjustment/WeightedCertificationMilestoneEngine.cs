namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted certification milestone calculation.</summary>
public interface IWeightedCertificationMilestoneAdjuster
{
    string Describe();
    double Adjust(WeightedCertificationMilestone item, double valueValue);
}

public sealed record TieredWeightedCertificationMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IWeightedCertificationMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedCertificationMilestone item, double valueValue)
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

public sealed record RoundingWeightedCertificationMilestoneAdjuster(double Step) : IWeightedCertificationMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedCertificationMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedCertificationMilestoneAdjuster(double Minimum) : IWeightedCertificationMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedCertificationMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameWeightedCertificationMilestoneAdjuster(Dictionary<string, double> Factors) : IWeightedCertificationMilestoneAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedCertificationMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WeightedCertificationMilestoneUpdatedAtOffsetAdjuster(double Weight) : IWeightedCertificationMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(WeightedCertificationMilestone item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the weighted certification milestone steps in order and keeps a trace of each.</summary>
public sealed class WeightedCertificationMilestoneEngine(List<IWeightedCertificationMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedCertificationMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedCertificationMilestoneEngine DefaultWeightedCertificationMilestoneEngine()
    {
        return new WeightedCertificationMilestoneEngine(
            new List<IWeightedCertificationMilestoneAdjuster>
            {
                new TieredWeightedCertificationMilestoneAdjuster(
                    new List<double> { 63.0, 2633.0, 7650.0, 9903.0 },
                    new List<double> { 1.05, 1.01, 1.0, 1.09 }),
                new RoundingWeightedCertificationMilestoneAdjuster(0.05),
                new MinimumWeightedCertificationMilestoneAdjuster(1.6),
                new NameWeightedCertificationMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.95, ["online"] = 0.94, ["west"] = 1.21 }),
                new WeightedCertificationMilestoneUpdatedAtOffsetAdjuster(1.02),
            },
            new List<string>(),
            4.1,
            61172.5);
    }

    public double Price(WeightedCertificationMilestone item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedCertificationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
