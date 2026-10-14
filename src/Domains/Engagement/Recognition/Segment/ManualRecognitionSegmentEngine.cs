namespace Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>One step of the manual recognition segment calculation.</summary>
public interface IManualRecognitionSegmentStep
{
    string Describe();
    double Adjust(ManualRecognitionSegment item, double valueValue);
}

public sealed record TitleManualRecognitionSegmentAdjuster(Dictionary<string, double> Factors) : IManualRecognitionSegmentStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ManualRecognitionSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentManualRecognitionSegmentAdjuster(double Percent) : IManualRecognitionSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualRecognitionSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualRecognitionSegmentAdjuster(double Minimum) : IManualRecognitionSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualRecognitionSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ManualRecognitionSegmentAmountOffsetAdjuster(double Weight) : IManualRecognitionSegmentStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ManualRecognitionSegment item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the manual recognition segment steps in order and keeps a trace of each.</summary>
public sealed class ManualRecognitionSegmentEngine(List<IManualRecognitionSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualRecognitionSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualRecognitionSegmentEngine DefaultManualRecognitionSegmentEngine()
    {
        return new ManualRecognitionSegmentEngine(
            new List<IManualRecognitionSegmentStep>
            {
                new TitleManualRecognitionSegmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.78, ["retail"] = 1.11 }),
                new PercentManualRecognitionSegmentAdjuster(-8.0),
                new MinimumManualRecognitionSegmentAdjuster(46.0),
                new ManualRecognitionSegmentAmountOffsetAdjuster(0.27),
            },
            new List<string>(),
            4.2,
            86767.9);
    }

    public double Compute(ManualRecognitionSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualRecognitionSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
