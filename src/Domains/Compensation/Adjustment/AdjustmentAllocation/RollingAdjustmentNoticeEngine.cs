namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling adjustment notice calculation.</summary>
public interface IRollingAdjustmentNoticeAdjuster
{
    string Describe();
    double Adjust(RollingAdjustmentNotice item, double valueValue);
}

public sealed record RollingAdjustmentNoticePriorityOffsetAdjuster(double Weight) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TitleRollingAdjustmentNoticeAdjuster(Dictionary<string, double> Factors) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingAdjustmentNoticeAdjuster(double Percent) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingAdjustmentNoticeAdjuster(double Step) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingAdjustmentNoticeAdjuster(List<double> Bounds, List<double> Rates) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
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

public sealed record CapRollingAdjustmentNoticeAdjuster(double Cap) : IRollingAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAdjustmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling adjustment notice steps in order and keeps a trace of each.</summary>
public sealed class RollingAdjustmentNoticeEngine(List<IRollingAdjustmentNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAdjustmentNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAdjustmentNoticeEngine DefaultRollingAdjustmentNoticeEngine()
    {
        return new RollingAdjustmentNoticeEngine(
            new List<IRollingAdjustmentNoticeAdjuster>
            {
                new RollingAdjustmentNoticePriorityOffsetAdjuster(0.95),
                new TitleRollingAdjustmentNoticeAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.71, ["retail"] = 1.07, ["west"] = 0.81 }),
                new PercentRollingAdjustmentNoticeAdjuster(4.1),
                new RoundingRollingAdjustmentNoticeAdjuster(0.05),
                new TieredRollingAdjustmentNoticeAdjuster(
                    new List<double> { 723.0, 2289.0, 3180.0 },
                    new List<double> { 0.96, 0.93, 1.07 }),
                new CapRollingAdjustmentNoticeAdjuster(5546.0),
            },
            new List<string>(),
            4.1,
            24473.6);
    }

    public double Run(RollingAdjustmentNotice item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAdjustmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
