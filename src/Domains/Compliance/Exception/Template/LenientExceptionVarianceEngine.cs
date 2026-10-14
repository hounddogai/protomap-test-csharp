namespace Acme.Workforce.Compliance.Exception.Template;

/// <summary>One step of the lenient exception variance calculation.</summary>
public interface ILenientExceptionVarianceStep
{
    string Describe();
    double Adjust(LenientExceptionVariance item, double valueValue);
}

public sealed record LenientExceptionVarianceCeilingOffsetAdjuster(double Weight) : ILenientExceptionVarianceStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(LenientExceptionVariance item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record TeamLenientExceptionVarianceAdjuster(Dictionary<string, double> Factors) : ILenientExceptionVarianceStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LenientExceptionVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLenientExceptionVarianceAdjuster(double Percent) : ILenientExceptionVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientExceptionVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient exception variance steps in order and keeps a trace of each.</summary>
public sealed class LenientExceptionVarianceEngine(List<ILenientExceptionVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientExceptionVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientExceptionVarianceEngine DefaultLenientExceptionVarianceEngine()
    {
        return new LenientExceptionVarianceEngine(
            new List<ILenientExceptionVarianceStep>
            {
                new LenientExceptionVarianceCeilingOffsetAdjuster(0.87),
                new TeamLenientExceptionVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.77, ["east"] = 0.75, ["west"] = 0.76 }),
                new PercentLenientExceptionVarianceAdjuster(-6.3),
            },
            new List<string>(),
            2.0,
            62993.1);
    }

    public double Evaluate(LenientExceptionVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientExceptionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
