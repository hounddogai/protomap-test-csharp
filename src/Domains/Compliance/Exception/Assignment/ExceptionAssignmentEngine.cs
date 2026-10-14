namespace Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>One step of the exception assignment calculation.</summary>
public interface IExceptionAssignmentStep
{
    string Describe();
    double Adjust(ExceptionAssignment item, double valueValue);
}

public sealed record LocaleExceptionAssignmentAdjuster(Dictionary<string, double> Factors) : IExceptionAssignmentStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ExceptionAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumExceptionAssignmentAdjuster(double Minimum) : IExceptionAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredExceptionAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionAssignment item, double valueValue)
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

public sealed record PercentExceptionAssignmentAdjuster(double Percent) : IExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ExceptionAssignmentRateOffsetAdjuster(double Weight) : IExceptionAssignmentStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(ExceptionAssignment item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the exception assignment steps in order and keeps a trace of each.</summary>
public sealed class ExceptionAssignmentEngine(List<IExceptionAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionAssignmentEngine DefaultExceptionAssignmentEngine()
    {
        return new ExceptionAssignmentEngine(
            new List<IExceptionAssignmentStep>
            {
                new LocaleExceptionAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.0, ["west"] = 0.84, ["central"] = 0.99 }),
                new MinimumExceptionAssignmentAdjuster(32.3),
                new TieredExceptionAssignmentAdjuster(
                    new List<double> { 48.0, 2851.0, 4291.0 },
                    new List<double> { 0.81, 0.94, 1.06 }),
                new PercentExceptionAssignmentAdjuster(-3.9),
                new ExceptionAssignmentRateOffsetAdjuster(0.85),
            },
            new List<string>(),
            2.3,
            66506.2);
    }

    public double Price(ExceptionAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
