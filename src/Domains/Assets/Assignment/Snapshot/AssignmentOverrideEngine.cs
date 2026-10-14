namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment override calculation.</summary>
public interface IAssignmentOverrideStep
{
    string Describe();
    double Adjust(AssignmentOverride item, double valueValue);
}

public sealed record AssignmentOverrideCeilingOffsetAdjuster(double Weight) : IAssignmentOverrideStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(AssignmentOverride item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record PercentAssignmentOverrideAdjuster(double Percent) : IAssignmentOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssignmentOverrideAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentOverride item, double valueValue)
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

public sealed record RoundingAssignmentOverrideAdjuster(double Step) : IAssignmentOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAssignmentOverrideAdjuster(double Cap) : IAssignmentOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment override steps in order and keeps a trace of each.</summary>
public sealed class AssignmentOverrideEngine(List<IAssignmentOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentOverrideEngine DefaultAssignmentOverrideEngine()
    {
        return new AssignmentOverrideEngine(
            new List<IAssignmentOverrideStep>
            {
                new AssignmentOverrideCeilingOffsetAdjuster(1.45),
                new PercentAssignmentOverrideAdjuster(-15.9),
                new TieredAssignmentOverrideAdjuster(
                    new List<double> { 1267.0, 2904.0, 7934.0 },
                    new List<double> { 0.95, 0.94, 1.09 }),
                new RoundingAssignmentOverrideAdjuster(0.25),
                new CapAssignmentOverrideAdjuster(46359.0),
            },
            new List<string>(),
            1.0,
            14567.9);
    }

    public double Run(AssignmentOverride item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
