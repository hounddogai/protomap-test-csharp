namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition claim calculation.</summary>
public interface IRecognitionClaimAdjuster
{
    string Describe();
    double Adjust(RecognitionClaim item, double valueValue);
}

public sealed record RoundingRecognitionClaimAdjuster(double Step) : IRecognitionClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRecognitionClaimAdjuster(double Minimum) : IRecognitionClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RecognitionClaimScoreOffsetAdjuster(double Weight) : IRecognitionClaimAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RecognitionClaim item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentRecognitionClaimAdjuster(double Percent) : IRecognitionClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelRecognitionClaimAdjuster(Dictionary<string, double> Factors) : IRecognitionClaimAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RecognitionClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the recognition claim steps in order and keeps a trace of each.</summary>
public sealed class RecognitionClaimEngine(List<IRecognitionClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionClaimEngine DefaultRecognitionClaimEngine()
    {
        return new RecognitionClaimEngine(
            new List<IRecognitionClaimAdjuster>
            {
                new RoundingRecognitionClaimAdjuster(0.5),
                new MinimumRecognitionClaimAdjuster(24.1),
                new RecognitionClaimScoreOffsetAdjuster(0.07),
                new PercentRecognitionClaimAdjuster(-14.9),
                new ChannelRecognitionClaimAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.04, ["south"] = 1.14, ["online"] = 1.0 }),
            },
            new List<string>(),
            4.4,
            85693.1);
    }

    public double Evaluate(RecognitionClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
