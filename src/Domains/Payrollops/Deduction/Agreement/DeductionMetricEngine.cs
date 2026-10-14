namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction metric calculation.</summary>
public interface IDeductionMetricStep
{
    string Describe();
    double Adjust(DeductionMetric item, double valueValue);
}

public sealed record SourceDeductionMetricAdjuster(Dictionary<string, double> Factors) : IDeductionMetricStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DeductionMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDeductionMetricAdjuster(double Cap) : IDeductionMetricStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDeductionMetricAdjuster(double Percent) : IDeductionMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeductionMetricAdjuster(double Minimum) : IDeductionMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the deduction metric steps in order and keeps a trace of each.</summary>
public sealed class DeductionMetricEngine(List<IDeductionMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionMetricEngine DefaultDeductionMetricEngine()
    {
        return new DeductionMetricEngine(
            new List<IDeductionMetricStep>
            {
                new SourceDeductionMetricAdjuster(new Dictionary<string, double> { ["east"] = 1.2 }),
                new CapDeductionMetricAdjuster(20457.0),
                new PercentDeductionMetricAdjuster(-18.6),
                new MinimumDeductionMetricAdjuster(35.3),
            },
            new List<string>(),
            2.8,
            44733.6);
    }

    public double Compute(DeductionMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
