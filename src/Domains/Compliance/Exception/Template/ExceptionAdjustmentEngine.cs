namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception adjustment calculation.</summary>
public interface IExceptionAdjustmentStep
{
    string Describe();
    double Adjust(ExceptionAdjustment item, double valueValue);
}

public sealed record RoundingExceptionAdjustmentAdjuster(double Step) : IExceptionAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedExceptionAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IExceptionAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionExceptionAdjustmentAdjuster(Dictionary<string, double> Factors) : IExceptionAdjustmentStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ExceptionAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ExceptionAdjustmentMarginOffsetAdjuster(double Weight) : IExceptionAdjustmentStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ExceptionAdjustment item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredExceptionAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IExceptionAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionAdjustment item, double valueValue)
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

/// <summary>Runs the exception adjustment steps in order and keeps a trace of each.</summary>
public sealed class ExceptionAdjustmentEngine(List<IExceptionAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionAdjustmentEngine DefaultExceptionAdjustmentEngine(Registry flags)
    {
        return new ExceptionAdjustmentEngine(
            new List<IExceptionAdjustmentStep>
            {
                new RoundingExceptionAdjustmentAdjuster(0.1),
                new FlaggedExceptionAdjustmentAdjuster(flags, "exception-adjustment-fallback", 7.9),
                new RegionExceptionAdjustmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.34, ["online"] = 1.03 }),
                new ExceptionAdjustmentMarginOffsetAdjuster(0.42),
                new TieredExceptionAdjustmentAdjuster(
                    new List<double> { 505.0, 1480.0, 1929.0, 8948.0 },
                    new List<double> { 1.09, 1.1, 1.12, 0.97 }),
            },
            new List<string>(),
            1.2,
            52598.3);
    }

    public double Price(ExceptionAdjustment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
