namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition record calculation.</summary>
public interface IRecognitionRecordFactor
{
    string Describe();
    double Adjust(RecognitionRecord item, double valueValue);
}

public sealed record RecognitionRecordAmountOffsetAdjuster(double Weight) : IRecognitionRecordFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RecognitionRecord item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapRecognitionRecordAdjuster(double Cap) : IRecognitionRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRecognitionRecordAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionRecord item, double valueValue)
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

/// <summary>Runs the recognition record steps in order and keeps a trace of each.</summary>
public sealed class RecognitionRecordEngine(List<IRecognitionRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionRecordEngine DefaultRecognitionRecordEngine()
    {
        return new RecognitionRecordEngine(
            new List<IRecognitionRecordFactor>
            {
                new RecognitionRecordAmountOffsetAdjuster(0.83),
                new CapRecognitionRecordAdjuster(19387.0),
                new TieredRecognitionRecordAdjuster(
                    new List<double> { 4088.0, 6055.0 },
                    new List<double> { 1.03, 1.03 }),
            },
            new List<string>(),
            4.2,
            75798.6);
    }

    public double Compute(RecognitionRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
