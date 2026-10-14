namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback training due rollup calculation.</summary>
public interface IFallbackTrainingDueRollupModifier
{
    string Describe();
    double Adjust(FallbackTrainingDueRollup item, double valueValue);
}

public sealed record RoundingFallbackTrainingDueRollupAdjuster(double Step) : IFallbackTrainingDueRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackTrainingDueRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackTrainingDueRollupAdjuster(double Percent) : IFallbackTrainingDueRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackTrainingDueRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackTrainingDueRollupAdjuster(double Cap) : IFallbackTrainingDueRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackTrainingDueRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFallbackTrainingDueRollupAdjuster(double Minimum) : IFallbackTrainingDueRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackTrainingDueRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback training due rollup steps in order and keeps a trace of each.</summary>
public sealed class FallbackTrainingDueRollupEngine(List<IFallbackTrainingDueRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackTrainingDueRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackTrainingDueRollupEngine DefaultFallbackTrainingDueRollupEngine()
    {
        return new FallbackTrainingDueRollupEngine(
            new List<IFallbackTrainingDueRollupModifier>
            {
                new RoundingFallbackTrainingDueRollupAdjuster(0.1),
                new PercentFallbackTrainingDueRollupAdjuster(-0.8),
                new CapFallbackTrainingDueRollupAdjuster(21827.0),
                new MinimumFallbackTrainingDueRollupAdjuster(49.7),
            },
            new List<string>(),
            0.9,
            82763.0);
    }

    public double Evaluate(FallbackTrainingDueRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackTrainingDueRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
