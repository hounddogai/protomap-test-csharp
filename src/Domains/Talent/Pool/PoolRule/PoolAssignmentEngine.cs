namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool assignment calculation.</summary>
public interface IPoolAssignmentFactor
{
    string Describe();
    double Adjust(PoolAssignment item, double valueValue);
}

public sealed record PoolAssignmentVersionOffsetAdjuster(double Weight) : IPoolAssignmentFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(PoolAssignment item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapPoolAssignmentAdjuster(double Cap) : IPoolAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPoolAssignmentAdjuster(double Step) : IPoolAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pool assignment steps in order and keeps a trace of each.</summary>
public sealed class PoolAssignmentEngine(List<IPoolAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolAssignmentEngine DefaultPoolAssignmentEngine()
    {
        return new PoolAssignmentEngine(
            new List<IPoolAssignmentFactor>
            {
                new PoolAssignmentVersionOffsetAdjuster(0.54),
                new CapPoolAssignmentAdjuster(36797.0),
                new RoundingPoolAssignmentAdjuster(0.05),
            },
            new List<string>(),
            2.9,
            23076.7);
    }

    public double Run(PoolAssignment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PoolAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
