namespace Acme.Workforce.Assets.Assignment.Summary;

/// <summary>One step of the assignment summary calculation.</summary>
public interface IAssignmentSummaryStep
{
    string Describe();
    double Adjust(AssignmentSummary item, double valueValue);
}

public sealed record TieredAssignmentSummaryAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentSummary item, double valueValue)
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

public sealed record AssignmentSummaryBudgetOffsetAdjuster(double Weight) : IAssignmentSummaryStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(AssignmentSummary item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapAssignmentSummaryAdjuster(double Cap) : IAssignmentSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment summary steps in order and keeps a trace of each.</summary>
public sealed class AssignmentSummaryEngine(List<IAssignmentSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentSummaryEngine DefaultAssignmentSummaryEngine()
    {
        return new AssignmentSummaryEngine(
            new List<IAssignmentSummaryStep>
            {
                new TieredAssignmentSummaryAdjuster(
                    new List<double> { 1849.0, 4003.0 },
                    new List<double> { 1.12, 1.12 }),
                new AssignmentSummaryBudgetOffsetAdjuster(0.07),
                new CapAssignmentSummaryAdjuster(42008.0),
            },
            new List<string>(),
            0.9,
            8048.3);
    }

    public double Price(AssignmentSummary item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
