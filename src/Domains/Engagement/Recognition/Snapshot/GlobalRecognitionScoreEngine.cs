namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global recognition score calculation.</summary>
public interface IGlobalRecognitionScoreModifier
{
    string Describe();
    double Adjust(GlobalRecognitionScore item, double valueValue);
}

public sealed record CapGlobalRecognitionScoreAdjuster(double Cap) : IGlobalRecognitionScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalRecognitionScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalRecognitionScoreAdjuster(double Step) : IGlobalRecognitionScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalRecognitionScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalRecognitionScoreAdjuster(Registry Flags, string Flag, double Percent) : IGlobalRecognitionScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalRecognitionScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalRecognitionScoreAdjuster(double Minimum) : IGlobalRecognitionScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalRecognitionScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalRecognitionScoreAdjuster(List<double> Bounds, List<double> Rates) : IGlobalRecognitionScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalRecognitionScore item, double valueValue)
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

/// <summary>Runs the global recognition score steps in order and keeps a trace of each.</summary>
public sealed class GlobalRecognitionScoreEngine(List<IGlobalRecognitionScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalRecognitionScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalRecognitionScoreEngine DefaultGlobalRecognitionScoreEngine(Registry flags)
    {
        return new GlobalRecognitionScoreEngine(
            new List<IGlobalRecognitionScoreModifier>
            {
                new CapGlobalRecognitionScoreAdjuster(19150.0),
                new RoundingGlobalRecognitionScoreAdjuster(0.05),
                new FlaggedGlobalRecognitionScoreAdjuster(flags, "global-recognition-score-fallback", -0.8),
                new MinimumGlobalRecognitionScoreAdjuster(2.9),
                new TieredGlobalRecognitionScoreAdjuster(
                    new List<double> { 2963.0, 6254.0 },
                    new List<double> { 0.97, 0.9 }),
            },
            new List<string>(),
            0.5,
            48007.5);
    }

    public double Run(GlobalRecognitionScore item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalRecognitionScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
