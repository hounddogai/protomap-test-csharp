namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pool schedule calculation.</summary>
public interface IStrictPoolScheduleAdjuster
{
    string Describe();
    double Adjust(StrictPoolSchedule item, double valueValue);
}

public sealed record MinimumStrictPoolScheduleAdjuster(double Minimum) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictPoolScheduleSequenceOffsetAdjuster(double Weight) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record TieredStrictPoolScheduleAdjuster(List<double> Bounds, List<double> Rates) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
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

public sealed record CapStrictPoolScheduleAdjuster(double Cap) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStrictPoolScheduleAdjuster(double Percent) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceStrictPoolScheduleAdjuster(Dictionary<string, double> Factors) : IStrictPoolScheduleAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictPoolSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict pool schedule steps in order and keeps a trace of each.</summary>
public sealed class StrictPoolScheduleEngine(List<IStrictPoolScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPoolScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPoolScheduleEngine DefaultStrictPoolScheduleEngine()
    {
        return new StrictPoolScheduleEngine(
            new List<IStrictPoolScheduleAdjuster>
            {
                new MinimumStrictPoolScheduleAdjuster(7.3),
                new StrictPoolScheduleSequenceOffsetAdjuster(0.37),
                new TieredStrictPoolScheduleAdjuster(
                    new List<double> { 3784.0, 8508.0, 8758.0 },
                    new List<double> { 0.83, 0.9, 1.18 }),
                new CapStrictPoolScheduleAdjuster(11181.0),
                new PercentStrictPoolScheduleAdjuster(-21.9),
                new SourceStrictPoolScheduleAdjuster(new Dictionary<string, double> { ["east"] = 0.74 }),
            },
            new List<string>(),
            3.2,
            39813.0);
    }

    public double Compute(StrictPoolSchedule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPoolSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
