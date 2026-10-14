namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment request calculation.</summary>
public interface IAssignmentRequestFactor
{
    string Describe();
    double Adjust(AssignmentRequest item, double valueValue);
}

public sealed record RoundingAssignmentRequestAdjuster(double Step) : IAssignmentRequestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAssignmentRequestAdjuster(Dictionary<string, double> Factors) : IAssignmentRequestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AssignmentRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAssignmentRequestAdjuster(double Cap) : IAssignmentRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAssignmentRequestAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentRequest item, double valueValue)
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

/// <summary>Runs the assignment request steps in order and keeps a trace of each.</summary>
public sealed class AssignmentRequestEngine(List<IAssignmentRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentRequestEngine DefaultAssignmentRequestEngine()
    {
        return new AssignmentRequestEngine(
            new List<IAssignmentRequestFactor>
            {
                new RoundingAssignmentRequestAdjuster(5.0),
                new LabelAssignmentRequestAdjuster(new Dictionary<string, double> { ["online"] = 0.79 }),
                new CapAssignmentRequestAdjuster(47759.0),
                new TieredAssignmentRequestAdjuster(
                    new List<double> { 3731.0, 4554.0, 6608.0 },
                    new List<double> { 1.11, 1.19, 0.94 }),
            },
            new List<string>(),
            1.1,
            93725.5);
    }

    public double Run(AssignmentRequest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
