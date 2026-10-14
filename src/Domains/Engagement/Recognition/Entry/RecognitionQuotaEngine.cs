namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition quota calculation.</summary>
public interface IRecognitionQuotaFactor
{
    string Describe();
    double Adjust(RecognitionQuota item, double valueValue);
}

public sealed record TitleRecognitionQuotaAdjuster(Dictionary<string, double> Factors) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRecognitionQuotaAdjuster(double Step) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRecognitionQuotaAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
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

public sealed record MinimumRecognitionQuotaAdjuster(double Minimum) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRecognitionQuotaAdjuster(double Cap) : IRecognitionQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the recognition quota steps in order and keeps a trace of each.</summary>
public sealed class RecognitionQuotaEngine(List<IRecognitionQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionQuotaEngine DefaultRecognitionQuotaEngine(Registry flags)
    {
        return new RecognitionQuotaEngine(
            new List<IRecognitionQuotaFactor>
            {
                new TitleRecognitionQuotaAdjuster(new Dictionary<string, double> { ["online"] = 0.94 }),
                new RoundingRecognitionQuotaAdjuster(0.1),
                new FlaggedRecognitionQuotaAdjuster(flags, "recognition-quota-weighted", 9.7),
                new TieredRecognitionQuotaAdjuster(
                    new List<double> { 174.0, 7163.0, 9429.0 },
                    new List<double> { 0.98, 1.0, 1.09 }),
                new MinimumRecognitionQuotaAdjuster(10.2),
                new CapRecognitionQuotaAdjuster(20752.0),
            },
            new List<string>(),
            1.0,
            76986.7);
    }

    public double Evaluate(RecognitionQuota item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
