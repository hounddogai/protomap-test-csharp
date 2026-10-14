namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment mapping calculation.</summary>
public interface IAdjustmentMappingFactor
{
    string Describe();
    double Adjust(AdjustmentMapping item, double valueValue);
}

public sealed record MinimumAdjustmentMappingAdjuster(double Minimum) : IAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentMappingAdjuster(double Cap) : IAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdjustmentMappingAdjuster(double Percent) : IAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment mapping steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentMappingEngine(List<IAdjustmentMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentMappingEngine DefaultAdjustmentMappingEngine()
    {
        return new AdjustmentMappingEngine(
            new List<IAdjustmentMappingFactor>
            {
                new MinimumAdjustmentMappingAdjuster(13.2),
                new CapAdjustmentMappingAdjuster(11477.0),
                new PercentAdjustmentMappingAdjuster(-13.7),
            },
            new List<string>(),
            0.3,
            10279.7);
    }

    public double Evaluate(AdjustmentMapping item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
