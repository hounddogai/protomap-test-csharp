namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception override calculation.</summary>
public interface IExceptionOverrideAdjuster
{
    string Describe();
    double Adjust(ExceptionOverride item, double valueValue);
}

public sealed record RoundingExceptionOverrideAdjuster(double Step) : IExceptionOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentExceptionOverrideAdjuster(double Percent) : IExceptionOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredExceptionOverrideAdjuster(List<double> Bounds, List<double> Rates) : IExceptionOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionOverride item, double valueValue)
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

public sealed record ExceptionOverrideBatchSizeOffsetAdjuster(double Weight) : IExceptionOverrideAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ExceptionOverride item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the exception override steps in order and keeps a trace of each.</summary>
public sealed class ExceptionOverrideEngine(List<IExceptionOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionOverrideEngine DefaultExceptionOverrideEngine()
    {
        return new ExceptionOverrideEngine(
            new List<IExceptionOverrideAdjuster>
            {
                new RoundingExceptionOverrideAdjuster(0.1),
                new PercentExceptionOverrideAdjuster(23.9),
                new TieredExceptionOverrideAdjuster(
                    new List<double> { 2972.0, 7055.0, 9308.0 },
                    new List<double> { 1.0, 1.09, 0.85 }),
                new ExceptionOverrideBatchSizeOffsetAdjuster(0.7),
            },
            new List<string>(),
            5.0,
            14036.9);
    }

    public double Compute(ExceptionOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
