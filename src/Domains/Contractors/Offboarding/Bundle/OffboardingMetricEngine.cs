namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding metric calculation.</summary>
public interface IOffboardingMetricStep
{
    string Describe();
    double Adjust(OffboardingMetric item, double valueValue);
}

public sealed record OffboardingMetricWeightOffsetAdjuster(double Weight) : IOffboardingMetricStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(OffboardingMetric item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingOffboardingMetricAdjuster(double Step) : IOffboardingMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentOffboardingMetricAdjuster(double Percent) : IOffboardingMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding metric steps in order and keeps a trace of each.</summary>
public sealed class OffboardingMetricEngine(List<IOffboardingMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingMetricEngine DefaultOffboardingMetricEngine()
    {
        return new OffboardingMetricEngine(
            new List<IOffboardingMetricStep>
            {
                new OffboardingMetricWeightOffsetAdjuster(1.55),
                new RoundingOffboardingMetricAdjuster(0.05),
                new PercentOffboardingMetricAdjuster(7.6),
            },
            new List<string>(),
            2.9,
            68535.4);
    }

    public double Evaluate(OffboardingMetric item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
