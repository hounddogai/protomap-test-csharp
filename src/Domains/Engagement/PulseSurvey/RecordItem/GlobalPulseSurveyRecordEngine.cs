namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>One step of the global pulse survey record calculation.</summary>
public interface IGlobalPulseSurveyRecordFactor
{
    string Describe();
    double Adjust(GlobalPulseSurveyRecord item, double valueValue);
}

public sealed record NameGlobalPulseSurveyRecordAdjuster(Dictionary<string, double> Factors) : IGlobalPulseSurveyRecordFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalPulseSurveyRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalPulseSurveyRecordAdjuster(double Minimum) : IGlobalPulseSurveyRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPulseSurveyRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalPulseSurveyRecordWeightOffsetAdjuster(double Weight) : IGlobalPulseSurveyRecordFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(GlobalPulseSurveyRecord item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the global pulse survey record steps in order and keeps a trace of each.</summary>
public sealed class GlobalPulseSurveyRecordEngine(List<IGlobalPulseSurveyRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPulseSurveyRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPulseSurveyRecordEngine DefaultGlobalPulseSurveyRecordEngine()
    {
        return new GlobalPulseSurveyRecordEngine(
            new List<IGlobalPulseSurveyRecordFactor>
            {
                new NameGlobalPulseSurveyRecordAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.77, ["online"] = 0.91 }),
                new MinimumGlobalPulseSurveyRecordAdjuster(30.2),
                new GlobalPulseSurveyRecordWeightOffsetAdjuster(1.12),
            },
            new List<string>(),
            3.0,
            19348.2);
    }

    public double Compute(GlobalPulseSurveyRecord item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPulseSurveyRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
