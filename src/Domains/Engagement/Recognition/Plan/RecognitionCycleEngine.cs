namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition cycle calculation.</summary>
public interface IRecognitionCycleFactor
{
    string Describe();
    double Adjust(RecognitionCycle item, double valueValue);
}

public sealed record RoundingRecognitionCycleAdjuster(double Step) : IRecognitionCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryRecognitionCycleAdjuster(Dictionary<string, double> Factors) : IRecognitionCycleFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RecognitionCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRecognitionCycleAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionCycleAdjuster(double Cap) : IRecognitionCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the recognition cycle steps in order and keeps a trace of each.</summary>
public sealed class RecognitionCycleEngine(List<IRecognitionCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionCycleEngine DefaultRecognitionCycleEngine(Registry flags)
    {
        return new RecognitionCycleEngine(
            new List<IRecognitionCycleFactor>
            {
                new RoundingRecognitionCycleAdjuster(5.0),
                new CategoryRecognitionCycleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.16, ["south"] = 1.1, ["west"] = 1.05 }),
                new FlaggedRecognitionCycleAdjuster(flags, "recognition-cycle-rolling", 8.2),
                new CapRecognitionCycleAdjuster(15605.0),
            },
            new List<string>(),
            0.4,
            22977.5);
    }

    public double Evaluate(RecognitionCycle item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
