namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped rate milestone calculation.</summary>
public interface ICappedRateMilestoneFactor
{
    string Describe();
    double Adjust(CappedRateMilestone item, double valueValue);
}

public sealed record MinimumCappedRateMilestoneAdjuster(double Minimum) : ICappedRateMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedRateMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedRateMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ICappedRateMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRateMilestone item, double valueValue)
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

public sealed record RoundingCappedRateMilestoneAdjuster(double Step) : ICappedRateMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRateMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped rate milestone steps in order and keeps a trace of each.</summary>
public sealed class CappedRateMilestoneEngine(List<ICappedRateMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRateMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRateMilestoneEngine DefaultCappedRateMilestoneEngine()
    {
        return new CappedRateMilestoneEngine(
            new List<ICappedRateMilestoneFactor>
            {
                new MinimumCappedRateMilestoneAdjuster(16.3),
                new TieredCappedRateMilestoneAdjuster(
                    new List<double> { 6398.0, 6963.0 },
                    new List<double> { 1.09, 0.8 }),
                new RoundingCappedRateMilestoneAdjuster(5.0),
            },
            new List<string>(),
            4.8,
            55622.0);
    }

    public double Evaluate(CappedRateMilestone item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRateMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
