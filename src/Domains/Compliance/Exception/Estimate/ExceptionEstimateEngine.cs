namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the exception estimate calculation.</summary>
public interface IExceptionEstimateModifier
{
    string Describe();
    double Adjust(ExceptionEstimate item, double valueValue);
}

public sealed record FlaggedExceptionEstimateAdjuster(Registry Flags, string Flag, double Percent) : IExceptionEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumExceptionEstimateAdjuster(double Minimum) : IExceptionEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ExceptionEstimateRatioOffsetAdjuster(double Weight) : IExceptionEstimateModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(ExceptionEstimate item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the exception estimate steps in order and keeps a trace of each.</summary>
public sealed class ExceptionEstimateEngine(List<IExceptionEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionEstimateEngine DefaultExceptionEstimateEngine(Registry flags)
    {
        return new ExceptionEstimateEngine(
            new List<IExceptionEstimateModifier>
            {
                new FlaggedExceptionEstimateAdjuster(flags, "exception-estimate-pending", -2.0),
                new MinimumExceptionEstimateAdjuster(46.5),
                new ExceptionEstimateRatioOffsetAdjuster(0.71),
            },
            new List<string>(),
            4.1,
            39936.5);
    }

    public double Evaluate(ExceptionEstimate item)
    {
        var current = item.EffectiveLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ExceptionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
