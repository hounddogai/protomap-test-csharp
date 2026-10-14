namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the exception bundle calculation.</summary>
public interface IExceptionBundleStep
{
    string Describe();
    double Adjust(ExceptionBundle item, double valueValue);
}

public sealed record PercentExceptionBundleAdjuster(double Percent) : IExceptionBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedExceptionBundleAdjuster(Registry Flags, string Flag, double Percent) : IExceptionBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ExceptionBundleRateOffsetAdjuster(double Weight) : IExceptionBundleStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(ExceptionBundle item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the exception bundle steps in order and keeps a trace of each.</summary>
public sealed class ExceptionBundleEngine(List<IExceptionBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionBundleEngine DefaultExceptionBundleEngine(Registry flags)
    {
        return new ExceptionBundleEngine(
            new List<IExceptionBundleStep>
            {
                new PercentExceptionBundleAdjuster(-7.4),
                new FlaggedExceptionBundleAdjuster(flags, "exception-bundle-batched", 6.4),
                new ExceptionBundleRateOffsetAdjuster(1.6),
            },
            new List<string>(),
            4.5,
            52085.3);
    }

    public double Run(ExceptionBundle item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
