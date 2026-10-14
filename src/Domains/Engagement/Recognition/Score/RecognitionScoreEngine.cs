namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition score calculation.</summary>
public interface IRecognitionScoreFactor
{
    string Describe();
    double Adjust(RecognitionScore item, double valueValue);
}

public sealed record LabelRecognitionScoreAdjuster(Dictionary<string, double> Factors) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RecognitionScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRecognitionScoreAdjuster(double Minimum) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRecognitionScoreAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionScore item, double valueValue)
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

public sealed record RecognitionScoreRateOffsetAdjuster(double Weight) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RecognitionScore item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record PercentRecognitionScoreAdjuster(double Percent) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionScoreAdjuster(double Cap) : IRecognitionScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the recognition score steps in order and keeps a trace of each.</summary>
public sealed class RecognitionScoreEngine(List<IRecognitionScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionScoreEngine DefaultRecognitionScoreEngine()
    {
        return new RecognitionScoreEngine(
            new List<IRecognitionScoreFactor>
            {
                new LabelRecognitionScoreAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.27, ["east"] = 0.71 }),
                new MinimumRecognitionScoreAdjuster(26.8),
                new TieredRecognitionScoreAdjuster(
                    new List<double> { 2006.0, 6621.0 },
                    new List<double> { 0.9, 0.96 }),
                new RecognitionScoreRateOffsetAdjuster(0.07),
                new PercentRecognitionScoreAdjuster(19.7),
                new CapRecognitionScoreAdjuster(25016.0),
            },
            new List<string>(),
            3.1,
            94275.7);
    }

    public double Price(RecognitionScore item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
