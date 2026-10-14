namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual scenario assignment calculation.</summary>
public interface IManualScenarioAssignmentStep
{
    string Describe();
    double Adjust(ManualScenarioAssignment item, double valueValue);
}

public sealed record ManualScenarioAssignmentPriorityOffsetAdjuster(double Weight) : IManualScenarioAssignmentStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ManualScenarioAssignment item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record PercentManualScenarioAssignmentAdjuster(double Percent) : IManualScenarioAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualScenarioAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualScenarioAssignmentAdjuster(double Step) : IManualScenarioAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualScenarioAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualScenarioAssignmentAdjuster(double Minimum) : IManualScenarioAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualScenarioAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual scenario assignment steps in order and keeps a trace of each.</summary>
public sealed class ManualScenarioAssignmentEngine(List<IManualScenarioAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualScenarioAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualScenarioAssignmentEngine DefaultManualScenarioAssignmentEngine()
    {
        return new ManualScenarioAssignmentEngine(
            new List<IManualScenarioAssignmentStep>
            {
                new ManualScenarioAssignmentPriorityOffsetAdjuster(1.46),
                new PercentManualScenarioAssignmentAdjuster(15.1),
                new RoundingManualScenarioAssignmentAdjuster(0.1),
                new MinimumManualScenarioAssignmentAdjuster(37.2),
            },
            new List<string>(),
            1.0,
            46273.4);
    }

    public double Run(ManualScenarioAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualScenarioAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
