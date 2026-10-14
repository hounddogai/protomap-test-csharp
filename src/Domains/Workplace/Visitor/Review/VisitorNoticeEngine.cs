namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor notice calculation.</summary>
public interface IVisitorNoticeFactor
{
    string Describe();
    double Adjust(VisitorNotice item, double valueValue);
}

public sealed record VisitorNoticeEndsAtOffsetAdjuster(double Weight) : IVisitorNoticeFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(VisitorNotice item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CountryVisitorNoticeAdjuster(Dictionary<string, double> Factors) : IVisitorNoticeFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(VisitorNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedVisitorNoticeAdjuster(Registry Flags, string Flag, double Percent) : IVisitorNoticeFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor notice steps in order and keeps a trace of each.</summary>
public sealed class VisitorNoticeEngine(List<IVisitorNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorNoticeEngine DefaultVisitorNoticeEngine(Registry flags)
    {
        return new VisitorNoticeEngine(
            new List<IVisitorNoticeFactor>
            {
                new VisitorNoticeEndsAtOffsetAdjuster(1.52),
                new CountryVisitorNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.71, ["north"] = 0.86, ["south"] = 0.88 }),
                new FlaggedVisitorNoticeAdjuster(flags, "visitor-notice-batched", 4.2),
            },
            new List<string>(),
            3.0,
            98952.7);
    }

    public double Compute(VisitorNotice item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
