namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool metric calculation.</summary>
public interface IBonusPoolMetricFactor
{
    string Describe();
    double Adjust(BonusPoolMetric item, double valueValue);
}

public sealed record PercentBonusPoolMetricAdjuster(double Percent) : IBonusPoolMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBonusPoolMetricAdjuster(double Cap) : IBonusPoolMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBonusPoolMetricAdjuster(double Minimum) : IBonusPoolMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the bonus pool metric steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolMetricEngine(List<IBonusPoolMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolMetricEngine DefaultBonusPoolMetricEngine()
    {
        return new BonusPoolMetricEngine(
            new List<IBonusPoolMetricFactor>
            {
                new PercentBonusPoolMetricAdjuster(13.4),
                new CapBonusPoolMetricAdjuster(21485.0),
                new MinimumBonusPoolMetricAdjuster(13.0),
            },
            new List<string>(),
            0.3,
            45483.2);
    }

    public double Price(BonusPoolMetric item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
