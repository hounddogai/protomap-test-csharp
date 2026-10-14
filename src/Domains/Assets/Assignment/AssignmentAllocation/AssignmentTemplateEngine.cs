namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment template calculation.</summary>
public interface IAssignmentTemplateStep
{
    string Describe();
    double Adjust(AssignmentTemplate item, double valueValue);
}

public sealed record TieredAssignmentTemplateAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentTemplate item, double valueValue)
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

public sealed record RoundingAssignmentTemplateAdjuster(double Step) : IAssignmentTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAssignmentTemplateAdjuster(double Minimum) : IAssignmentTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the assignment template steps in order and keeps a trace of each.</summary>
public sealed class AssignmentTemplateEngine(List<IAssignmentTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentTemplateEngine DefaultAssignmentTemplateEngine()
    {
        return new AssignmentTemplateEngine(
            new List<IAssignmentTemplateStep>
            {
                new TieredAssignmentTemplateAdjuster(
                    new List<double> { 4283.0, 6729.0 },
                    new List<double> { 0.9, 0.86 }),
                new RoundingAssignmentTemplateAdjuster(1.0),
                new MinimumAssignmentTemplateAdjuster(17.3),
            },
            new List<string>(),
            4.9,
            16704.4);
    }

    public double Price(AssignmentTemplate item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
