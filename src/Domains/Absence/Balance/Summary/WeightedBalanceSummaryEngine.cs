namespace Acme.Workforce.Absence.Balance.Summary;

/// <summary>One step of the weighted balance summary calculation.</summary>
public interface IWeightedBalanceSummaryAdjuster
{
    string Describe();
    double Adjust(WeightedBalanceSummary item, double valueValue);
}

public sealed record TieredWeightedBalanceSummaryAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBalanceSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBalanceSummary item, double valueValue)
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

public sealed record PercentWeightedBalanceSummaryAdjuster(double Percent) : IWeightedBalanceSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedBalanceSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedBalanceSummaryAdjuster(double Minimum) : IWeightedBalanceSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBalanceSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted balance summary steps in order and keeps a trace of each.</summary>
public sealed class WeightedBalanceSummaryEngine(List<IWeightedBalanceSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBalanceSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBalanceSummaryEngine DefaultWeightedBalanceSummaryEngine()
    {
        return new WeightedBalanceSummaryEngine(
            new List<IWeightedBalanceSummaryAdjuster>
            {
                new TieredWeightedBalanceSummaryAdjuster(
                    new List<double> { 6618.0, 9288.0, 9661.0, 9828.0 },
                    new List<double> { 0.91, 0.88, 1.09, 0.93 }),
                new PercentWeightedBalanceSummaryAdjuster(-5.6),
                new MinimumWeightedBalanceSummaryAdjuster(40.1),
            },
            new List<string>(),
            2.6,
            18733.7);
    }

    public double Price(WeightedBalanceSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBalanceSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
