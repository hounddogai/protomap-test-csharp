namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan batch calculation.</summary>
public interface IReturnPlanBatchModifier
{
    string Describe();
    double Adjust(ReturnPlanBatch item, double valueValue);
}

public sealed record FlaggedReturnPlanBatchAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReturnPlanBatchAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanBatch item, double valueValue)
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

public sealed record ReturnPlanBatchVersionOffsetAdjuster(double Weight) : IReturnPlanBatchModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ReturnPlanBatch item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentReturnPlanBatchAdjuster(double Percent) : IReturnPlanBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReturnPlanBatchAdjuster(double Step) : IReturnPlanBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the return plan batch steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanBatchEngine(List<IReturnPlanBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanBatchEngine DefaultReturnPlanBatchEngine(Registry flags)
    {
        return new ReturnPlanBatchEngine(
            new List<IReturnPlanBatchModifier>
            {
                new FlaggedReturnPlanBatchAdjuster(flags, "return-plan-batch-manual", 3.7),
                new TieredReturnPlanBatchAdjuster(
                    new List<double> { 986.0, 6919.0, 8445.0 },
                    new List<double> { 1.01, 0.86, 0.99 }),
                new ReturnPlanBatchVersionOffsetAdjuster(1.14),
                new PercentReturnPlanBatchAdjuster(15.6),
                new RoundingReturnPlanBatchAdjuster(0.1),
            },
            new List<string>(),
            0.3,
            68926.9);
    }

    public double Evaluate(ReturnPlanBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
