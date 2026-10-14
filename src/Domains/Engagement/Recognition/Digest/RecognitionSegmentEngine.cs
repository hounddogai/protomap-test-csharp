namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition segment calculation.</summary>
public interface IRecognitionSegmentFactor
{
    string Describe();
    double Adjust(RecognitionSegment item, double valueValue);
}

public sealed record RoundingRecognitionSegmentAdjuster(double Step) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentRecognitionSegmentAdjuster(Dictionary<string, double> Factors) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RecognitionSegmentMinimumOffsetAdjuster(double Weight) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record PercentRecognitionSegmentAdjuster(double Percent) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionSegmentAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
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

public sealed record FlaggedRecognitionSegmentAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition segment steps in order and keeps a trace of each.</summary>
public sealed class RecognitionSegmentEngine(List<IRecognitionSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionSegmentEngine DefaultRecognitionSegmentEngine(Registry flags)
    {
        return new RecognitionSegmentEngine(
            new List<IRecognitionSegmentFactor>
            {
                new RoundingRecognitionSegmentAdjuster(5.0),
                new SegmentRecognitionSegmentAdjuster(new Dictionary<string, double> { ["north"] = 0.86 }),
                new RecognitionSegmentMinimumOffsetAdjuster(1.68),
                new PercentRecognitionSegmentAdjuster(-5.2),
                new TieredRecognitionSegmentAdjuster(
                    new List<double> { 565.0, 1949.0, 8767.0, 8952.0 },
                    new List<double> { 0.83, 1.01, 1.04, 1.1 }),
                new FlaggedRecognitionSegmentAdjuster(flags, "recognition-segment-pending", 9.9),
            },
            new List<string>(),
            2.6,
            33729.8);
    }

    public double Evaluate(RecognitionSegment item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
