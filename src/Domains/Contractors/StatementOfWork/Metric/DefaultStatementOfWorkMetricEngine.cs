namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default statement of work metric calculation.</summary>
public interface IDefaultStatementOfWorkMetricModifier
{
    string Describe();
    double Adjust(DefaultStatementOfWorkMetric item, double valueValue);
}

public sealed record FlaggedDefaultStatementOfWorkMetricAdjuster(Registry Flags, string Flag, double Percent) : IDefaultStatementOfWorkMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultStatementOfWorkMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultStatementOfWorkMetricAdjuster(double Minimum) : IDefaultStatementOfWorkMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultStatementOfWorkMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultStatementOfWorkMetricAdjuster(double Percent) : IDefaultStatementOfWorkMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultStatementOfWorkMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryDefaultStatementOfWorkMetricAdjuster(Dictionary<string, double> Factors) : IDefaultStatementOfWorkMetricModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultStatementOfWorkMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultStatementOfWorkMetricAdjuster(double Cap) : IDefaultStatementOfWorkMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultStatementOfWorkMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default statement of work metric steps in order and keeps a trace of each.</summary>
public sealed class DefaultStatementOfWorkMetricEngine(List<IDefaultStatementOfWorkMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultStatementOfWorkMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultStatementOfWorkMetricEngine DefaultDefaultStatementOfWorkMetricEngine(Registry flags)
    {
        return new DefaultStatementOfWorkMetricEngine(
            new List<IDefaultStatementOfWorkMetricModifier>
            {
                new FlaggedDefaultStatementOfWorkMetricAdjuster(
                    flags,
                    "default-statement-of-work-metric-rolling",
                    1.6),
                new MinimumDefaultStatementOfWorkMetricAdjuster(38.7),
                new PercentDefaultStatementOfWorkMetricAdjuster(-24.4),
                new CountryDefaultStatementOfWorkMetricAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.09 }),
                new CapDefaultStatementOfWorkMetricAdjuster(15336.0),
            },
            new List<string>(),
            2.2,
            91346.0);
    }

    public double Price(DefaultStatementOfWorkMetric item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultStatementOfWorkMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
