namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive adjustment notice calculation.</summary>
public interface IAdaptiveAdjustmentNoticeModifier
{
    string Describe();
    double Adjust(AdaptiveAdjustmentNotice item, double valueValue);
}

public sealed record MinimumAdaptiveAdjustmentNoticeAdjuster(double Minimum) : IAdaptiveAdjustmentNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveAdjustmentNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveAdjustmentNoticeStartsAtOffsetAdjuster(double Weight) : IAdaptiveAdjustmentNoticeModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(AdaptiveAdjustmentNotice item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record RoundingAdaptiveAdjustmentNoticeAdjuster(double Step) : IAdaptiveAdjustmentNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveAdjustmentNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive adjustment notice steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAdjustmentNoticeEngine(List<IAdaptiveAdjustmentNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAdjustmentNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAdjustmentNoticeEngine DefaultAdaptiveAdjustmentNoticeEngine()
    {
        return new AdaptiveAdjustmentNoticeEngine(
            new List<IAdaptiveAdjustmentNoticeModifier>
            {
                new MinimumAdaptiveAdjustmentNoticeAdjuster(41.7),
                new AdaptiveAdjustmentNoticeStartsAtOffsetAdjuster(1.37),
                new RoundingAdaptiveAdjustmentNoticeAdjuster(0.05),
            },
            new List<string>(),
            4.9,
            27377.4);
    }

    public double Compute(AdaptiveAdjustmentNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAdjustmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
