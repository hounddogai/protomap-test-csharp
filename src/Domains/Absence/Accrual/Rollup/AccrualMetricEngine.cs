namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual metric calculation.</summary>
public interface IAccrualMetricAdjuster
{
    string Describe();
    double Adjust(AccrualMetric item, double valueValue);
}

public sealed record MinimumAccrualMetricAdjuster(double Minimum) : IAccrualMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AccrualMetricGraceDaysOffsetAdjuster(double Weight) : IAccrualMetricAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(AccrualMetric item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentAccrualMetricAdjuster(double Percent) : IAccrualMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the accrual metric steps in order and keeps a trace of each.</summary>
public sealed class AccrualMetricEngine(List<IAccrualMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualMetricEngine DefaultAccrualMetricEngine()
    {
        return new AccrualMetricEngine(
            new List<IAccrualMetricAdjuster>
            {
                new MinimumAccrualMetricAdjuster(9.6),
                new AccrualMetricGraceDaysOffsetAdjuster(0.19),
                new PercentAccrualMetricAdjuster(13.9),
            },
            new List<string>(),
            1.6,
            88511.5);
    }

    public double Run(AccrualMetric item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
