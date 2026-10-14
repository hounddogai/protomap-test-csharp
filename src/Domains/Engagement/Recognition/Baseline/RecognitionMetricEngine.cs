namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition metric calculation.</summary>
public interface IRecognitionMetricModifier
{
    string Describe();
    double Adjust(RecognitionMetric item, double valueValue);
}

public sealed record FlaggedRecognitionMetricAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionMetricAdjuster(double Cap) : IRecognitionMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRecognitionMetricAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionMetricModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionMetric item, double valueValue)
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

public sealed record RecognitionMetricGraceDaysOffsetAdjuster(double Weight) : IRecognitionMetricModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(RecognitionMetric item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record RoundingRecognitionMetricAdjuster(double Step) : IRecognitionMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the recognition metric steps in order and keeps a trace of each.</summary>
public sealed class RecognitionMetricEngine(List<IRecognitionMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionMetricEngine DefaultRecognitionMetricEngine(Registry flags)
    {
        return new RecognitionMetricEngine(
            new List<IRecognitionMetricModifier>
            {
                new FlaggedRecognitionMetricAdjuster(flags, "recognition-metric-fallback", 7.3),
                new CapRecognitionMetricAdjuster(49706.0),
                new TieredRecognitionMetricAdjuster(
                    new List<double> { 2180.0, 7866.0 },
                    new List<double> { 1.13, 0.84 }),
                new RecognitionMetricGraceDaysOffsetAdjuster(0.82),
                new RoundingRecognitionMetricAdjuster(0.05),
            },
            new List<string>(),
            4.2,
            35270.8);
    }

    public double Price(RecognitionMetric item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
