namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception metric calculation.</summary>
public interface IExceptionMetricFactor
{
    string Describe();
    double Adjust(ExceptionMetric item, double valueValue);
}

public sealed record RoundingExceptionMetricAdjuster(double Step) : IExceptionMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ExceptionMetricTenureMonthsOffsetAdjuster(double Weight) : IExceptionMetricFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(ExceptionMetric item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TieredExceptionMetricAdjuster(List<double> Bounds, List<double> Rates) : IExceptionMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionMetric item, double valueValue)
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

/// <summary>Runs the exception metric steps in order and keeps a trace of each.</summary>
public sealed class ExceptionMetricEngine(List<IExceptionMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionMetricEngine DefaultExceptionMetricEngine()
    {
        return new ExceptionMetricEngine(
            new List<IExceptionMetricFactor>
            {
                new RoundingExceptionMetricAdjuster(1.0),
                new ExceptionMetricTenureMonthsOffsetAdjuster(0.11),
                new TieredExceptionMetricAdjuster(
                    new List<double> { 208.0, 2675.0 },
                    new List<double> { 0.94, 1.14 }),
            },
            new List<string>(),
            3.8,
            7182.2);
    }

    public double Compute(ExceptionMetric item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
