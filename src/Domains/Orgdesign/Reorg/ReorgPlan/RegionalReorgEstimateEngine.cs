namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>One step of the regional reorg estimate calculation.</summary>
public interface IRegionalReorgEstimateStep
{
    string Describe();
    double Adjust(RegionalReorgEstimate item, double valueValue);
}

public sealed record TieredRegionalReorgEstimateAdjuster(List<double> Bounds, List<double> Rates) : IRegionalReorgEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalReorgEstimate item, double valueValue)
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

public sealed record SegmentRegionalReorgEstimateAdjuster(Dictionary<string, double> Factors) : IRegionalReorgEstimateStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RegionalReorgEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalReorgEstimateAdjuster(double Minimum) : IRegionalReorgEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalReorgEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalReorgEstimateAdjuster(double Percent) : IRegionalReorgEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalReorgEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalReorgEstimateAdjuster(double Cap) : IRegionalReorgEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalReorgEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional reorg estimate steps in order and keeps a trace of each.</summary>
public sealed class RegionalReorgEstimateEngine(List<IRegionalReorgEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalReorgEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalReorgEstimateEngine DefaultRegionalReorgEstimateEngine()
    {
        return new RegionalReorgEstimateEngine(
            new List<IRegionalReorgEstimateStep>
            {
                new TieredRegionalReorgEstimateAdjuster(
                    new List<double> { 5252.0, 5383.0, 9308.0 },
                    new List<double> { 0.99, 1.15, 1.17 }),
                new SegmentRegionalReorgEstimateAdjuster(new Dictionary<string, double> { ["south"] = 0.84 }),
                new MinimumRegionalReorgEstimateAdjuster(30.1),
                new PercentRegionalReorgEstimateAdjuster(2.4),
                new CapRegionalReorgEstimateAdjuster(40332.0),
            },
            new List<string>(),
            0.9,
            32648.5);
    }

    public double Price(RegionalReorgEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalReorgEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
