namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance cycle calculation.</summary>
public interface IBalanceCycleFactor
{
    string Describe();
    double Adjust(BalanceCycle item, double valueValue);
}

public sealed record BalanceCycleThresholdOffsetAdjuster(double Weight) : IBalanceCycleFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(BalanceCycle item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record PercentBalanceCycleAdjuster(double Percent) : IBalanceCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceCycleAdjuster(double Minimum) : IBalanceCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the balance cycle steps in order and keeps a trace of each.</summary>
public sealed class BalanceCycleEngine(List<IBalanceCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceCycleEngine DefaultBalanceCycleEngine()
    {
        return new BalanceCycleEngine(
            new List<IBalanceCycleFactor>
            {
                new BalanceCycleThresholdOffsetAdjuster(1.36),
                new PercentBalanceCycleAdjuster(-9.3),
                new MinimumBalanceCycleAdjuster(2.4),
            },
            new List<string>(),
            2.2,
            7970.9);
    }

    public double Evaluate(BalanceCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
