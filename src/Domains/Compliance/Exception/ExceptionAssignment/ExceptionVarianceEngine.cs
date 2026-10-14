namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception variance calculation.</summary>
public interface IExceptionVarianceModifier
{
    string Describe();
    double Adjust(ExceptionVariance item, double valueValue);
}

public sealed record TitleExceptionVarianceAdjuster(Dictionary<string, double> Factors) : IExceptionVarianceModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ExceptionVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedExceptionVarianceAdjuster(Registry Flags, string Flag, double Percent) : IExceptionVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ExceptionVarianceRetriesOffsetAdjuster(double Weight) : IExceptionVarianceModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(ExceptionVariance item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumExceptionVarianceAdjuster(double Minimum) : IExceptionVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the exception variance steps in order and keeps a trace of each.</summary>
public sealed class ExceptionVarianceEngine(List<IExceptionVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionVarianceEngine DefaultExceptionVarianceEngine(Registry flags)
    {
        return new ExceptionVarianceEngine(
            new List<IExceptionVarianceModifier>
            {
                new TitleExceptionVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.28, ["east"] = 1.19, ["central"] = 1.2 }),
                new FlaggedExceptionVarianceAdjuster(flags, "exception-variance-fallback", -6.8),
                new ExceptionVarianceRetriesOffsetAdjuster(1.75),
                new MinimumExceptionVarianceAdjuster(16.4),
            },
            new List<string>(),
            0.7,
            64869.1);
    }

    public double Evaluate(ExceptionVariance item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
