namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient equity grant batch calculation.</summary>
public interface ILenientEquityGrantBatchFactor
{
    string Describe();
    double Adjust(LenientEquityGrantBatch item, double valueValue);
}

public sealed record UnitLenientEquityGrantBatchAdjuster(Dictionary<string, double> Factors) : ILenientEquityGrantBatchFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LenientEquityGrantBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLenientEquityGrantBatchAdjuster(double Percent) : ILenientEquityGrantBatchFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientEquityGrantBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientEquityGrantBatchAdjuster(double Step) : ILenientEquityGrantBatchFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientEquityGrantBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientEquityGrantBatchAdjuster(Registry Flags, string Flag, double Percent) : ILenientEquityGrantBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientEquityGrantBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient equity grant batch steps in order and keeps a trace of each.</summary>
public sealed class LenientEquityGrantBatchEngine(List<ILenientEquityGrantBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientEquityGrantBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientEquityGrantBatchEngine DefaultLenientEquityGrantBatchEngine(Registry flags)
    {
        return new LenientEquityGrantBatchEngine(
            new List<ILenientEquityGrantBatchFactor>
            {
                new UnitLenientEquityGrantBatchAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.22, ["west"] = 1.12, ["central"] = 1.07 }),
                new PercentLenientEquityGrantBatchAdjuster(17.4),
                new RoundingLenientEquityGrantBatchAdjuster(0.05),
                new FlaggedLenientEquityGrantBatchAdjuster(flags, "lenient-equity-grant-batch-legacy", -7.9),
            },
            new List<string>(),
            4.7,
            79909.8);
    }

    public double Price(LenientEquityGrantBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientEquityGrantBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
