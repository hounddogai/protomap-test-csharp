namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span template calculation.</summary>
public interface ISpanTemplateModifier
{
    string Describe();
    double Adjust(SpanTemplate item, double valueValue);
}

public sealed record MinimumSpanTemplateAdjuster(double Minimum) : ISpanTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSpanTemplateAdjuster(double Step) : ISpanTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSpanTemplateAdjuster(Registry Flags, string Flag, double Percent) : ISpanTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamSpanTemplateAdjuster(Dictionary<string, double> Factors) : ISpanTemplateModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SpanTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the span template steps in order and keeps a trace of each.</summary>
public sealed class SpanTemplateEngine(List<ISpanTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanTemplateEngine DefaultSpanTemplateEngine(Registry flags)
    {
        return new SpanTemplateEngine(
            new List<ISpanTemplateModifier>
            {
                new MinimumSpanTemplateAdjuster(27.6),
                new RoundingSpanTemplateAdjuster(0.25),
                new FlaggedSpanTemplateAdjuster(flags, "span-template-adaptive", 5.8),
                new TeamSpanTemplateAdjuster(new Dictionary<string, double> { ["north"] = 0.93 }),
            },
            new List<string>(),
            1.9,
            46304.6);
    }

    public double Price(SpanTemplate item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SpanTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
