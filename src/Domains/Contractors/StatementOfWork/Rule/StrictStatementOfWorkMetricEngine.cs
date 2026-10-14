namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict statement of work metric calculation.</summary>
public interface IStrictStatementOfWorkMetricFactor
{
    string Describe();
    double Adjust(StrictStatementOfWorkMetric item, double valueValue);
}

public sealed record MinimumStrictStatementOfWorkMetricAdjuster(double Minimum) : IStrictStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictStatementOfWorkMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictStatementOfWorkMetricAdjuster(Registry Flags, string Flag, double Percent) : IStrictStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictStatementOfWorkMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictStatementOfWorkMetricAdjuster(double Percent) : IStrictStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictStatementOfWorkMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictStatementOfWorkMetricAdjuster(double Step) : IStrictStatementOfWorkMetricFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictStatementOfWorkMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict statement of work metric steps in order and keeps a trace of each.</summary>
public sealed class StrictStatementOfWorkMetricEngine(List<IStrictStatementOfWorkMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictStatementOfWorkMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictStatementOfWorkMetricEngine DefaultStrictStatementOfWorkMetricEngine(Registry flags)
    {
        return new StrictStatementOfWorkMetricEngine(
            new List<IStrictStatementOfWorkMetricFactor>
            {
                new MinimumStrictStatementOfWorkMetricAdjuster(48.4),
                new FlaggedStrictStatementOfWorkMetricAdjuster(flags, "strict-statement-of-work-metric-batched", 4.8),
                new PercentStrictStatementOfWorkMetricAdjuster(10.7),
                new RoundingStrictStatementOfWorkMetricAdjuster(0.5),
            },
            new List<string>(),
            4.3,
            63604.7);
    }

    public double Evaluate(StrictStatementOfWorkMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictStatementOfWorkMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
