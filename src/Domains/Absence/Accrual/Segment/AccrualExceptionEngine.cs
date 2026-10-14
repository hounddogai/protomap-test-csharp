namespace Acme.Workforce.Absence.Accrual.Segment;

/// <summary>One step of the accrual exception calculation.</summary>
public interface IAccrualExceptionModifier
{
    string Describe();
    double Adjust(AccrualException item, double valueValue);
}

public sealed record CapAccrualExceptionAdjuster(double Cap) : IAccrualExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAccrualExceptionAdjuster(double Percent) : IAccrualExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAccrualExceptionAdjuster(Dictionary<string, double> Factors) : IAccrualExceptionModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccrualException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the accrual exception steps in order and keeps a trace of each.</summary>
public sealed class AccrualExceptionEngine(List<IAccrualExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualExceptionEngine DefaultAccrualExceptionEngine()
    {
        return new AccrualExceptionEngine(
            new List<IAccrualExceptionModifier>
            {
                new CapAccrualExceptionAdjuster(33975.0),
                new PercentAccrualExceptionAdjuster(-8.8),
                new NameAccrualExceptionAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.08, ["online"] = 1.15 }),
            },
            new List<string>(),
            0.2,
            8278.7);
    }

    public double Run(AccrualException item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
