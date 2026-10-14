namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition slot calculation.</summary>
public interface IRecognitionSlotStep
{
    string Describe();
    double Adjust(RecognitionSlot item, double valueValue);
}

public sealed record RoundingRecognitionSlotAdjuster(double Step) : IRecognitionSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRecognitionSlotAdjuster(double Percent) : IRecognitionSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionSlotAdjuster(double Cap) : IRecognitionSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the recognition slot steps in order and keeps a trace of each.</summary>
public sealed class RecognitionSlotEngine(List<IRecognitionSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionSlotEngine DefaultRecognitionSlotEngine()
    {
        return new RecognitionSlotEngine(
            new List<IRecognitionSlotStep>
            {
                new RoundingRecognitionSlotAdjuster(0.25),
                new PercentRecognitionSlotAdjuster(-0.2),
                new CapRecognitionSlotAdjuster(15163.0),
            },
            new List<string>(),
            3.5,
            68134.1);
    }

    public double Run(RecognitionSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
