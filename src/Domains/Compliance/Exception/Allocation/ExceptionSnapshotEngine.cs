namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception snapshot calculation.</summary>
public interface IExceptionSnapshotFactor
{
    string Describe();
    double Adjust(ExceptionSnapshot item, double valueValue);
}

public sealed record FlaggedExceptionSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelExceptionSnapshotAdjuster(Dictionary<string, double> Factors) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentExceptionSnapshotAdjuster(double Percent) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingExceptionSnapshotAdjuster(double Step) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredExceptionSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
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

public sealed record ExceptionSnapshotCreatedAtOffsetAdjuster(double Weight) : IExceptionSnapshotFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(ExceptionSnapshot item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the exception snapshot steps in order and keeps a trace of each.</summary>
public sealed class ExceptionSnapshotEngine(List<IExceptionSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionSnapshotEngine DefaultExceptionSnapshotEngine(Registry flags)
    {
        return new ExceptionSnapshotEngine(
            new List<IExceptionSnapshotFactor>
            {
                new FlaggedExceptionSnapshotAdjuster(flags, "exception-snapshot-seasonal", -1.9),
                new LabelExceptionSnapshotAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.22, ["north"] = 1.08 }),
                new PercentExceptionSnapshotAdjuster(8.5),
                new RoundingExceptionSnapshotAdjuster(1.0),
                new TieredExceptionSnapshotAdjuster(
                    new List<double> { 7376.0, 8569.0 },
                    new List<double> { 1.05, 0.85 }),
                new ExceptionSnapshotCreatedAtOffsetAdjuster(1.57),
            },
            new List<string>(),
            4.0,
            31081.9);
    }

    public double Price(ExceptionSnapshot item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
