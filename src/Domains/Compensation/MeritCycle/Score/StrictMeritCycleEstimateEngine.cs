namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict merit cycle estimate calculation.</summary>
public interface IStrictMeritCycleEstimateFactor
{
    string Describe();
    double Adjust(StrictMeritCycleEstimate item, double valueValue);
}

public sealed record FlaggedStrictMeritCycleEstimateAdjuster(Registry Flags, string Flag, double Percent) : IStrictMeritCycleEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictMeritCycleEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictMeritCycleEstimateAdjuster(double Minimum) : IStrictMeritCycleEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictMeritCycleEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictMeritCycleEstimateAmountOffsetAdjuster(double Weight) : IStrictMeritCycleEstimateFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(StrictMeritCycleEstimate item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentStrictMeritCycleEstimateAdjuster(double Percent) : IStrictMeritCycleEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictMeritCycleEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict merit cycle estimate steps in order and keeps a trace of each.</summary>
public sealed class StrictMeritCycleEstimateEngine(List<IStrictMeritCycleEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictMeritCycleEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictMeritCycleEstimateEngine DefaultStrictMeritCycleEstimateEngine(Registry flags)
    {
        return new StrictMeritCycleEstimateEngine(
            new List<IStrictMeritCycleEstimateFactor>
            {
                new FlaggedStrictMeritCycleEstimateAdjuster(flags, "strict-merit-cycle-estimate-primary", 9.3),
                new MinimumStrictMeritCycleEstimateAdjuster(27.5),
                new StrictMeritCycleEstimateAmountOffsetAdjuster(0.85),
                new PercentStrictMeritCycleEstimateAdjuster(-3.4),
            },
            new List<string>(),
            1.4,
            87333.7);
    }

    public double Evaluate(StrictMeritCycleEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictMeritCycleEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
