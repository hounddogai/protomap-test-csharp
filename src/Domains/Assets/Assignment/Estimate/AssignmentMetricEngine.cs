namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment metric calculation.</summary>
public interface IAssignmentMetricAdjuster
{
    string Describe();
    double Adjust(AssignmentMetric item, double valueValue);
}

public sealed record TieredAssignmentMetricAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentMetric item, double valueValue)
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

public sealed record MinimumAssignmentMetricAdjuster(double Minimum) : IAssignmentMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAssignmentMetricAdjuster(double Step) : IAssignmentMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the assignment metric steps in order and keeps a trace of each.</summary>
public sealed class AssignmentMetricEngine(List<IAssignmentMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentMetricEngine DefaultAssignmentMetricEngine()
    {
        return new AssignmentMetricEngine(
            new List<IAssignmentMetricAdjuster>
            {
                new TieredAssignmentMetricAdjuster(
                    new List<double> { 978.0, 2083.0, 8248.0 },
                    new List<double> { 0.92, 1.12, 0.8 }),
                new MinimumAssignmentMetricAdjuster(46.1),
                new RoundingAssignmentMetricAdjuster(0.5),
            },
            new List<string>(),
            3.1,
            73418.2);
    }

    public double Compute(AssignmentMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
