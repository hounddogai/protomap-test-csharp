namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant schedule calculation.</summary>
public interface IEquityGrantScheduleAdjuster
{
    string Describe();
    double Adjust(EquityGrantSchedule item, double valueValue);
}

public sealed record CapEquityGrantScheduleAdjuster(double Cap) : IEquityGrantScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumEquityGrantScheduleAdjuster(double Minimum) : IEquityGrantScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record EquityGrantScheduleMarginOffsetAdjuster(double Weight) : IEquityGrantScheduleAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(EquityGrantSchedule item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the equity grant schedule steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantScheduleEngine(List<IEquityGrantScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantScheduleEngine DefaultEquityGrantScheduleEngine()
    {
        return new EquityGrantScheduleEngine(
            new List<IEquityGrantScheduleAdjuster>
            {
                new CapEquityGrantScheduleAdjuster(19775.0),
                new MinimumEquityGrantScheduleAdjuster(25.2),
                new EquityGrantScheduleMarginOffsetAdjuster(0.51),
            },
            new List<string>(),
            3.7,
            25625.1);
    }

    public double Evaluate(EquityGrantSchedule item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
