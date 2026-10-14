namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg cycle calculation.</summary>
public interface IReorgCycleModifier
{
    string Describe();
    double Adjust(ReorgCycle item, double valueValue);
}

public sealed record DepartmentReorgCycleAdjuster(Dictionary<string, double> Factors) : IReorgCycleModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ReorgCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentReorgCycleAdjuster(double Percent) : IReorgCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReorgCycleAdjuster(double Step) : IReorgCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the reorg cycle steps in order and keeps a trace of each.</summary>
public sealed class ReorgCycleEngine(List<IReorgCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgCycleEngine DefaultReorgCycleEngine()
    {
        return new ReorgCycleEngine(
            new List<IReorgCycleModifier>
            {
                new DepartmentReorgCycleAdjuster(new Dictionary<string, double> { ["online"] = 1.32 }),
                new PercentReorgCycleAdjuster(9.2),
                new RoundingReorgCycleAdjuster(0.1),
            },
            new List<string>(),
            3.9,
            41278.6);
    }

    public double Evaluate(ReorgCycle item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
