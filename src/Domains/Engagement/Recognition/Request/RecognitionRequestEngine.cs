namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition request calculation.</summary>
public interface IRecognitionRequestFactor
{
    string Describe();
    double Adjust(RecognitionRequest item, double valueValue);
}

public sealed record MinimumRecognitionRequestAdjuster(double Minimum) : IRecognitionRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRecognitionRequestAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RecognitionRequestAmountOffsetAdjuster(double Weight) : IRecognitionRequestFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RecognitionRequest item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the recognition request steps in order and keeps a trace of each.</summary>
public sealed class RecognitionRequestEngine(List<IRecognitionRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionRequestEngine DefaultRecognitionRequestEngine(Registry flags)
    {
        return new RecognitionRequestEngine(
            new List<IRecognitionRequestFactor>
            {
                new MinimumRecognitionRequestAdjuster(23.5),
                new FlaggedRecognitionRequestAdjuster(flags, "recognition-request-batched", 7.1),
                new RecognitionRequestAmountOffsetAdjuster(0.42),
            },
            new List<string>(),
            3.8,
            37052.8);
    }

    public double Price(RecognitionRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
