namespace Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>One step of the adjustment notice calculation.</summary>
public interface IAdjustmentNoticeAdjuster
{
    string Describe();
    double Adjust(AdjustmentNotice item, double valueValue);
}

public sealed record AdjustmentNoticeVersionOffsetAdjuster(double Weight) : IAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AdjustmentNotice item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumAdjustmentNoticeAdjuster(double Minimum) : IAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentNoticeAdjuster(double Cap) : IAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelAdjustmentNoticeAdjuster(Dictionary<string, double> Factors) : IAdjustmentNoticeAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdjustmentNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment notice steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentNoticeEngine(List<IAdjustmentNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentNoticeEngine DefaultAdjustmentNoticeEngine()
    {
        return new AdjustmentNoticeEngine(
            new List<IAdjustmentNoticeAdjuster>
            {
                new AdjustmentNoticeVersionOffsetAdjuster(1.26),
                new MinimumAdjustmentNoticeAdjuster(10.9),
                new CapAdjustmentNoticeAdjuster(34635.0),
                new LabelAdjustmentNoticeAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.21, ["west"] = 0.74, ["central"] = 1.05 }),
            },
            new List<string>(),
            1.8,
            59215.6);
    }

    public double Price(AdjustmentNotice item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
