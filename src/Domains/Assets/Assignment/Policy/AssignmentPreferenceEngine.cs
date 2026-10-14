namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>One step of the assignment preference calculation.</summary>
public interface IAssignmentPreferenceAdjuster
{
    string Describe();
    double Adjust(AssignmentPreference item, double valueValue);
}

public sealed record PercentAssignmentPreferenceAdjuster(double Percent) : IAssignmentPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssignmentPreferenceAdjuster(double Minimum) : IAssignmentPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryAssignmentPreferenceAdjuster(Dictionary<string, double> Factors) : IAssignmentPreferenceAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AssignmentPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAssignmentPreferenceAdjuster(double Cap) : IAssignmentPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment preference steps in order and keeps a trace of each.</summary>
public sealed class AssignmentPreferenceEngine(List<IAssignmentPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentPreferenceEngine DefaultAssignmentPreferenceEngine()
    {
        return new AssignmentPreferenceEngine(
            new List<IAssignmentPreferenceAdjuster>
            {
                new PercentAssignmentPreferenceAdjuster(-8.1),
                new MinimumAssignmentPreferenceAdjuster(21.1),
                new CategoryAssignmentPreferenceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.15, ["east"] = 0.92, ["online"] = 1.3 }),
                new CapAssignmentPreferenceAdjuster(49801.0),
            },
            new List<string>(),
            0.6,
            65534.1);
    }

    public double Run(AssignmentPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
