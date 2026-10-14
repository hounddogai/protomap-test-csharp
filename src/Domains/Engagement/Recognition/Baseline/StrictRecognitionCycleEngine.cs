namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict recognition cycle calculation.</summary>
public interface IStrictRecognitionCycleStep
{
    string Describe();
    double Adjust(StrictRecognitionCycle item, double valueValue);
}

public sealed record RoundingStrictRecognitionCycleAdjuster(double Step) : IStrictRecognitionCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictRecognitionCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictRecognitionCycleLimitOffsetAdjuster(double Weight) : IStrictRecognitionCycleStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(StrictRecognitionCycle item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredStrictRecognitionCycleAdjuster(List<double> Bounds, List<double> Rates) : IStrictRecognitionCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictRecognitionCycle item, double valueValue)
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

public sealed record PercentStrictRecognitionCycleAdjuster(double Percent) : IStrictRecognitionCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictRecognitionCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStrictRecognitionCycleAdjuster(Registry Flags, string Flag, double Percent) : IStrictRecognitionCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictRecognitionCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict recognition cycle steps in order and keeps a trace of each.</summary>
public sealed class StrictRecognitionCycleEngine(List<IStrictRecognitionCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictRecognitionCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictRecognitionCycleEngine DefaultStrictRecognitionCycleEngine(Registry flags)
    {
        return new StrictRecognitionCycleEngine(
            new List<IStrictRecognitionCycleStep>
            {
                new RoundingStrictRecognitionCycleAdjuster(0.1),
                new StrictRecognitionCycleLimitOffsetAdjuster(0.48),
                new TieredStrictRecognitionCycleAdjuster(
                    new List<double> { 584.0, 3148.0, 4120.0, 5581.0 },
                    new List<double> { 0.89, 1.1, 0.83, 1.12 }),
                new PercentStrictRecognitionCycleAdjuster(6.7),
                new FlaggedStrictRecognitionCycleAdjuster(flags, "strict-recognition-cycle-lenient", 7.0),
            },
            new List<string>(),
            4.4,
            81868.1);
    }

    public double Compute(StrictRecognitionCycle item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictRecognitionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
