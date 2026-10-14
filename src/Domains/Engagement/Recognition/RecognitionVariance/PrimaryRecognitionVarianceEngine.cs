namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary recognition variance calculation.</summary>
public interface IPrimaryRecognitionVarianceStep
{
    string Describe();
    double Adjust(PrimaryRecognitionVariance item, double valueValue);
}

public sealed record PrimaryRecognitionVarianceRetriesOffsetAdjuster(double Weight) : IPrimaryRecognitionVarianceStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PrimaryRecognitionVariance item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumPrimaryRecognitionVarianceAdjuster(double Minimum) : IPrimaryRecognitionVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryRecognitionVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPrimaryRecognitionVarianceAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryRecognitionVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryRecognitionVariance item, double valueValue)
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

/// <summary>Runs the primary recognition variance steps in order and keeps a trace of each.</summary>
public sealed class PrimaryRecognitionVarianceEngine(List<IPrimaryRecognitionVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryRecognitionVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryRecognitionVarianceEngine DefaultPrimaryRecognitionVarianceEngine()
    {
        return new PrimaryRecognitionVarianceEngine(
            new List<IPrimaryRecognitionVarianceStep>
            {
                new PrimaryRecognitionVarianceRetriesOffsetAdjuster(1.87),
                new MinimumPrimaryRecognitionVarianceAdjuster(35.7),
                new TieredPrimaryRecognitionVarianceAdjuster(
                    new List<double> { 6127.0, 9741.0 },
                    new List<double> { 1.08, 1.17 }),
            },
            new List<string>(),
            3.5,
            28629.0);
    }

    public double Run(PrimaryRecognitionVariance item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryRecognitionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
