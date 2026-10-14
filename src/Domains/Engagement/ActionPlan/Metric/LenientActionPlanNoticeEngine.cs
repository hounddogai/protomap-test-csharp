namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient action plan notice calculation.</summary>
public interface ILenientActionPlanNoticeStep
{
    string Describe();
    double Adjust(LenientActionPlanNotice item, double valueValue);
}

public sealed record LenientActionPlanNoticeTenureMonthsOffsetAdjuster(double Weight) : ILenientActionPlanNoticeStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LenientActionPlanNotice item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record SegmentLenientActionPlanNoticeAdjuster(Dictionary<string, double> Factors) : ILenientActionPlanNoticeStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LenientActionPlanNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientActionPlanNoticeAdjuster(double Cap) : ILenientActionPlanNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientActionPlanNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient action plan notice steps in order and keeps a trace of each.</summary>
public sealed class LenientActionPlanNoticeEngine(List<ILenientActionPlanNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientActionPlanNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientActionPlanNoticeEngine DefaultLenientActionPlanNoticeEngine()
    {
        return new LenientActionPlanNoticeEngine(
            new List<ILenientActionPlanNoticeStep>
            {
                new LenientActionPlanNoticeTenureMonthsOffsetAdjuster(0.72),
                new SegmentLenientActionPlanNoticeAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["retail"] = 1.05, ["central"] = 1.0 }),
                new CapLenientActionPlanNoticeAdjuster(15073.0),
            },
            new List<string>(),
            3.5,
            66229.4);
    }

    public double Compute(LenientActionPlanNotice item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientActionPlanNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
