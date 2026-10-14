namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span notice calculation.</summary>
public interface ISpanNoticeFactor
{
    string Describe();
    double Adjust(SpanNotice item, double valueValue);
}

public sealed record RoundingSpanNoticeAdjuster(double Step) : ISpanNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SpanNoticeMaximumOffsetAdjuster(double Weight) : ISpanNoticeFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TitleSpanNoticeAdjuster(Dictionary<string, double> Factors) : ISpanNoticeFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSpanNoticeAdjuster(double Minimum) : ISpanNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSpanNoticeAdjuster(double Percent) : ISpanNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSpanNoticeAdjuster(double Cap) : ISpanNoticeFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span notice steps in order and keeps a trace of each.</summary>
public sealed class SpanNoticeEngine(List<ISpanNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanNoticeEngine DefaultSpanNoticeEngine()
    {
        return new SpanNoticeEngine(
            new List<ISpanNoticeFactor>
            {
                new RoundingSpanNoticeAdjuster(1.0),
                new SpanNoticeMaximumOffsetAdjuster(0.29),
                new TitleSpanNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["retail"] = 1.23, ["east"] = 0.79 }),
                new MinimumSpanNoticeAdjuster(23.8),
                new PercentSpanNoticeAdjuster(-23.0),
                new CapSpanNoticeAdjuster(45146.0),
            },
            new List<string>(),
            3.0,
            42358.9);
    }

    public double Evaluate(SpanNotice item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SpanNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
