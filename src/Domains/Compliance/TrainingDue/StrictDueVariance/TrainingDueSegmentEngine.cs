namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>One step of the training due segment calculation.</summary>
public interface ITrainingDueSegmentFactor
{
    string Describe();
    double Adjust(TrainingDueSegment item, double valueValue);
}

public sealed record CountryTrainingDueSegmentAdjuster(Dictionary<string, double> Factors) : ITrainingDueSegmentFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(TrainingDueSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TrainingDueSegmentFloorValueOffsetAdjuster(double Weight) : ITrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(TrainingDueSegment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumTrainingDueSegmentAdjuster(double Minimum) : ITrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTrainingDueSegmentAdjuster(double Cap) : ITrainingDueSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the training due segment steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueSegmentEngine(List<ITrainingDueSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueSegmentEngine DefaultTrainingDueSegmentEngine()
    {
        return new TrainingDueSegmentEngine(
            new List<ITrainingDueSegmentFactor>
            {
                new CountryTrainingDueSegmentAdjuster(new Dictionary<string, double> { ["online"] = 1.21 }),
                new TrainingDueSegmentFloorValueOffsetAdjuster(1.05),
                new MinimumTrainingDueSegmentAdjuster(48.0),
                new CapTrainingDueSegmentAdjuster(7106.0),
            },
            new List<string>(),
            4.7,
            21371.6);
    }

    public double Compute(TrainingDueSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
