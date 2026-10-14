namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the exception baseline calculation.</summary>
public interface IExceptionBaselineFactor
{
    string Describe();
    double Adjust(ExceptionBaseline item, double valueValue);
}

public sealed record TieredExceptionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IExceptionBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionBaseline item, double valueValue)
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

public sealed record LabelExceptionBaselineAdjuster(Dictionary<string, double> Factors) : IExceptionBaselineFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ExceptionBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapExceptionBaselineAdjuster(double Cap) : IExceptionBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedExceptionBaselineAdjuster(Registry Flags, string Flag, double Percent) : IExceptionBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception baseline steps in order and keeps a trace of each.</summary>
public sealed class ExceptionBaselineEngine(List<IExceptionBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionBaselineEngine DefaultExceptionBaselineEngine(Registry flags)
    {
        return new ExceptionBaselineEngine(
            new List<IExceptionBaselineFactor>
            {
                new TieredExceptionBaselineAdjuster(
                    new List<double> { 1791.0, 4263.0, 5595.0, 7033.0 },
                    new List<double> { 0.98, 1.18, 1.17, 0.94 }),
                new LabelExceptionBaselineAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.0, ["online"] = 1.31, ["north"] = 0.92 }),
                new CapExceptionBaselineAdjuster(2427.0),
                new FlaggedExceptionBaselineAdjuster(flags, "exception-baseline-tiered", 4.7),
            },
            new List<string>(),
            3.7,
            90104.1);
    }

    public double Price(ExceptionBaseline item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
