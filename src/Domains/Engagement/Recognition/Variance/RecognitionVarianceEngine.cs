namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition variance calculation.</summary>
public interface IRecognitionVarianceFactor
{
    string Describe();
    double Adjust(RecognitionVariance item, double valueValue);
}

public sealed record FlaggedRecognitionVarianceAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionVarianceAdjuster(double Cap) : IRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRecognitionVarianceAdjuster(double Percent) : IRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRecognitionVarianceAdjuster(double Step) : IRecognitionVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the recognition variance steps in order and keeps a trace of each.</summary>
public sealed class RecognitionVarianceEngine(List<IRecognitionVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionVarianceEngine DefaultRecognitionVarianceEngine(Registry flags)
    {
        return new RecognitionVarianceEngine(
            new List<IRecognitionVarianceFactor>
            {
                new FlaggedRecognitionVarianceAdjuster(flags, "recognition-variance-capped", 7.0),
                new CapRecognitionVarianceAdjuster(24535.0),
                new PercentRecognitionVarianceAdjuster(-12.9),
                new RoundingRecognitionVarianceAdjuster(0.25),
            },
            new List<string>(),
            4.9,
            3793.8);
    }

    public double Run(RecognitionVariance item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
