namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception group calculation.</summary>
public interface IExceptionGroupAdjuster
{
    string Describe();
    double Adjust(ExceptionGroup item, double valueValue);
}

public sealed record ChannelExceptionGroupAdjuster(Dictionary<string, double> Factors) : IExceptionGroupAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ExceptionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumExceptionGroupAdjuster(double Minimum) : IExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ExceptionGroupThresholdOffsetAdjuster(double Weight) : IExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ExceptionGroup item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingExceptionGroupAdjuster(double Step) : IExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapExceptionGroupAdjuster(double Cap) : IExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the exception group steps in order and keeps a trace of each.</summary>
public sealed class ExceptionGroupEngine(List<IExceptionGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionGroupEngine DefaultExceptionGroupEngine()
    {
        return new ExceptionGroupEngine(
            new List<IExceptionGroupAdjuster>
            {
                new ChannelExceptionGroupAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.83, ["west"] = 1.3, ["central"] = 0.85 }),
                new MinimumExceptionGroupAdjuster(12.5),
                new ExceptionGroupThresholdOffsetAdjuster(1.09),
                new RoundingExceptionGroupAdjuster(0.05),
                new CapExceptionGroupAdjuster(11908.0),
            },
            new List<string>(),
            4.2,
            77938.5);
    }

    public double Run(ExceptionGroup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
