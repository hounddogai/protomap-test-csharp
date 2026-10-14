namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global exception snapshot calculation.</summary>
public interface IGlobalExceptionSnapshotAdjuster
{
    string Describe();
    double Adjust(GlobalExceptionSnapshot item, double valueValue);
}

public sealed record CapGlobalExceptionSnapshotAdjuster(double Cap) : IGlobalExceptionSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalExceptionSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumGlobalExceptionSnapshotAdjuster(double Minimum) : IGlobalExceptionSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalExceptionSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGlobalExceptionSnapshotAdjuster(double Step) : IGlobalExceptionSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalExceptionSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredGlobalExceptionSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IGlobalExceptionSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalExceptionSnapshot item, double valueValue)
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

public sealed record PercentGlobalExceptionSnapshotAdjuster(double Percent) : IGlobalExceptionSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalExceptionSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global exception snapshot steps in order and keeps a trace of each.</summary>
public sealed class GlobalExceptionSnapshotEngine(List<IGlobalExceptionSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalExceptionSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalExceptionSnapshotEngine DefaultGlobalExceptionSnapshotEngine()
    {
        return new GlobalExceptionSnapshotEngine(
            new List<IGlobalExceptionSnapshotAdjuster>
            {
                new CapGlobalExceptionSnapshotAdjuster(16468.0),
                new MinimumGlobalExceptionSnapshotAdjuster(4.4),
                new RoundingGlobalExceptionSnapshotAdjuster(0.25),
                new TieredGlobalExceptionSnapshotAdjuster(
                    new List<double> { 5435.0, 6009.0, 7187.0 },
                    new List<double> { 0.93, 1.13, 0.97 }),
                new PercentGlobalExceptionSnapshotAdjuster(-9.3),
            },
            new List<string>(),
            1.2,
            54151.1);
    }

    public double Compute(GlobalExceptionSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalExceptionSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
