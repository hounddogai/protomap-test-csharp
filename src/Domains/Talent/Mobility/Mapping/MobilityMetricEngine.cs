namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility metric calculation.</summary>
public interface IMobilityMetricStep
{
    string Describe();
    double Adjust(MobilityMetric item, double valueValue);
}

public sealed record CapMobilityMetricAdjuster(double Cap) : IMobilityMetricStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MobilityMetricQuantityOffsetAdjuster(double Weight) : IMobilityMetricStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(MobilityMetric item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumMobilityMetricAdjuster(double Minimum) : IMobilityMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentMobilityMetricAdjuster(Dictionary<string, double> Factors) : IMobilityMetricStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(MobilityMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentMobilityMetricAdjuster(double Percent) : IMobilityMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the mobility metric steps in order and keeps a trace of each.</summary>
public sealed class MobilityMetricEngine(List<IMobilityMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityMetricEngine DefaultMobilityMetricEngine()
    {
        return new MobilityMetricEngine(
            new List<IMobilityMetricStep>
            {
                new CapMobilityMetricAdjuster(7727.0),
                new MobilityMetricQuantityOffsetAdjuster(0.55),
                new MinimumMobilityMetricAdjuster(33.9),
                new DepartmentMobilityMetricAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["west"] = 0.81 }),
                new PercentMobilityMetricAdjuster(-2.3),
            },
            new List<string>(),
            0.9,
            99120.3);
    }

    public double Compute(MobilityMetric item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
