namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg group calculation.</summary>
public interface IReorgGroupFactor
{
    string Describe();
    double Adjust(ReorgGroup item, double valueValue);
}

public sealed record MinimumReorgGroupAdjuster(double Minimum) : IReorgGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapReorgGroupAdjuster(double Cap) : IReorgGroupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamReorgGroupAdjuster(Dictionary<string, double> Factors) : IReorgGroupFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ReorgGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the reorg group steps in order and keeps a trace of each.</summary>
public sealed class ReorgGroupEngine(List<IReorgGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgGroupEngine DefaultReorgGroupEngine()
    {
        return new ReorgGroupEngine(
            new List<IReorgGroupFactor>
            {
                new MinimumReorgGroupAdjuster(10.9),
                new CapReorgGroupAdjuster(28668.0),
                new TeamReorgGroupAdjuster(new Dictionary<string, double> { ["central"] = 1.03 }),
            },
            new List<string>(),
            0.5,
            62702.5);
    }

    public double Price(ReorgGroup item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
