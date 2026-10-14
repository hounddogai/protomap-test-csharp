namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception target calculation.</summary>
public interface IExceptionTargetAdjuster
{
    string Describe();
    double Adjust(ExceptionTarget item, double valueValue);
}

public sealed record PercentExceptionTargetAdjuster(double Percent) : IExceptionTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ExceptionTargetFloorValueOffsetAdjuster(double Weight) : IExceptionTargetAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ExceptionTarget item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record ChannelExceptionTargetAdjuster(Dictionary<string, double> Factors) : IExceptionTargetAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ExceptionTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedExceptionTargetAdjuster(Registry Flags, string Flag, double Percent) : IExceptionTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception target steps in order and keeps a trace of each.</summary>
public sealed class ExceptionTargetEngine(List<IExceptionTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionTargetEngine DefaultExceptionTargetEngine(Registry flags)
    {
        return new ExceptionTargetEngine(
            new List<IExceptionTargetAdjuster>
            {
                new PercentExceptionTargetAdjuster(4.3),
                new ExceptionTargetFloorValueOffsetAdjuster(1.89),
                new ChannelExceptionTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.25, ["north"] = 1.38 }),
                new FlaggedExceptionTargetAdjuster(flags, "exception-target-manual", -6.1),
            },
            new List<string>(),
            3.8,
            6657.0);
    }

    public double Compute(ExceptionTarget item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
