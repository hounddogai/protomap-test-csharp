namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default mobility target calculation.</summary>
public interface IDefaultMobilityTargetModifier
{
    string Describe();
    double Adjust(DefaultMobilityTarget item, double valueValue);
}

public sealed record CapDefaultMobilityTargetAdjuster(double Cap) : IDefaultMobilityTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultMobilityTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDefaultMobilityTargetAdjuster(double Percent) : IDefaultMobilityTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultMobilityTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultMobilityTargetRatingOffsetAdjuster(double Weight) : IDefaultMobilityTargetModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(DefaultMobilityTarget item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record DepartmentDefaultMobilityTargetAdjuster(Dictionary<string, double> Factors) : IDefaultMobilityTargetModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DefaultMobilityTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultMobilityTargetAdjuster(List<double> Bounds, List<double> Rates) : IDefaultMobilityTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultMobilityTarget item, double valueValue)
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

/// <summary>Runs the default mobility target steps in order and keeps a trace of each.</summary>
public sealed class DefaultMobilityTargetEngine(List<IDefaultMobilityTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultMobilityTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultMobilityTargetEngine DefaultDefaultMobilityTargetEngine()
    {
        return new DefaultMobilityTargetEngine(
            new List<IDefaultMobilityTargetModifier>
            {
                new CapDefaultMobilityTargetAdjuster(19776.0),
                new PercentDefaultMobilityTargetAdjuster(-9.0),
                new DefaultMobilityTargetRatingOffsetAdjuster(0.49),
                new DepartmentDefaultMobilityTargetAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.81, ["north"] = 1.37, ["south"] = 1.02 }),
                new TieredDefaultMobilityTargetAdjuster(
                    new List<double> { 6546.0, 8030.0 },
                    new List<double> { 1.05, 1.18 }),
            },
            new List<string>(),
            4.4,
            44852.5);
    }

    public double Evaluate(DefaultMobilityTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultMobilityTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
