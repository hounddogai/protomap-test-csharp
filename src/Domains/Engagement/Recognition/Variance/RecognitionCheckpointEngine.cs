namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition checkpoint calculation.</summary>
public interface IRecognitionCheckpointAdjuster
{
    string Describe();
    double Adjust(RecognitionCheckpoint item, double valueValue);
}

public sealed record PercentRecognitionCheckpointAdjuster(double Percent) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
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

public sealed record MinimumRecognitionCheckpointAdjuster(double Minimum) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamRecognitionCheckpointAdjuster(Dictionary<string, double> Factors) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRecognitionCheckpointAdjuster(double Step) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRecognitionCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition checkpoint steps in order and keeps a trace of each.</summary>
public sealed class RecognitionCheckpointEngine(List<IRecognitionCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionCheckpointEngine DefaultRecognitionCheckpointEngine(Registry flags)
    {
        return new RecognitionCheckpointEngine(
            new List<IRecognitionCheckpointAdjuster>
            {
                new PercentRecognitionCheckpointAdjuster(11.1),
                new TieredRecognitionCheckpointAdjuster(
                    new List<double> { 866.0, 1812.0, 8467.0 },
                    new List<double> { 0.82, 1.12, 1.14 }),
                new MinimumRecognitionCheckpointAdjuster(0.9),
                new TeamRecognitionCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.33, ["online"] = 0.79 }),
                new RoundingRecognitionCheckpointAdjuster(5.0),
                new FlaggedRecognitionCheckpointAdjuster(flags, "recognition-checkpoint-weighted", 7.1),
            },
            new List<string>(),
            4.6,
            54675.3);
    }

    public double Compute(RecognitionCheckpoint item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
