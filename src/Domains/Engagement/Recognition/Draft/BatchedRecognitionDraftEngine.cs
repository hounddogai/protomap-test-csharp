namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched recognition draft calculation.</summary>
public interface IBatchedRecognitionDraftStep
{
    string Describe();
    double Adjust(BatchedRecognitionDraft item, double valueValue);
}

public sealed record PercentBatchedRecognitionDraftAdjuster(double Percent) : IBatchedRecognitionDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedRecognitionDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedRecognitionDraftAdjuster(double Cap) : IBatchedRecognitionDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedRecognitionDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedRecognitionDraftAdjuster(List<double> Bounds, List<double> Rates) : IBatchedRecognitionDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedRecognitionDraft item, double valueValue)
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

public sealed record FlaggedBatchedRecognitionDraftAdjuster(Registry Flags, string Flag, double Percent) : IBatchedRecognitionDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedRecognitionDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched recognition draft steps in order and keeps a trace of each.</summary>
public sealed class BatchedRecognitionDraftEngine(List<IBatchedRecognitionDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRecognitionDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRecognitionDraftEngine DefaultBatchedRecognitionDraftEngine(Registry flags)
    {
        return new BatchedRecognitionDraftEngine(
            new List<IBatchedRecognitionDraftStep>
            {
                new PercentBatchedRecognitionDraftAdjuster(23.7),
                new CapBatchedRecognitionDraftAdjuster(6991.0),
                new TieredBatchedRecognitionDraftAdjuster(
                    new List<double> { 1770.0, 4677.0, 6647.0, 6943.0 },
                    new List<double> { 0.88, 0.97, 1.1, 0.81 }),
                new FlaggedBatchedRecognitionDraftAdjuster(flags, "batched-recognition-draft-legacy", -9.7),
            },
            new List<string>(),
            1.6,
            65693.3);
    }

    public double Price(BatchedRecognitionDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRecognitionDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
