namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global span draft calculation.</summary>
public interface IGlobalSpanDraftModifier
{
    string Describe();
    double Adjust(GlobalSpanDraft item, double valueValue);
}

public sealed record FlaggedGlobalSpanDraftAdjuster(Registry Flags, string Flag, double Percent) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleGlobalSpanDraftAdjuster(Dictionary<string, double> Factors) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGlobalSpanDraftAdjuster(double Step) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGlobalSpanDraftAdjuster(double Cap) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalSpanDraftAdjuster(double Percent) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalSpanDraftWeightOffsetAdjuster(double Weight) : IGlobalSpanDraftModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(GlobalSpanDraft item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the global span draft steps in order and keeps a trace of each.</summary>
public sealed class GlobalSpanDraftEngine(List<IGlobalSpanDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSpanDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSpanDraftEngine DefaultGlobalSpanDraftEngine(Registry flags)
    {
        return new GlobalSpanDraftEngine(
            new List<IGlobalSpanDraftModifier>
            {
                new FlaggedGlobalSpanDraftAdjuster(flags, "global-span-draft-rolling", -1.0),
                new TitleGlobalSpanDraftAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.92, ["central"] = 0.87 }),
                new RoundingGlobalSpanDraftAdjuster(0.1),
                new CapGlobalSpanDraftAdjuster(37732.0),
                new PercentGlobalSpanDraftAdjuster(22.9),
                new GlobalSpanDraftWeightOffsetAdjuster(0.36),
            },
            new List<string>(),
            4.2,
            13590.2);
    }

    public double Compute(GlobalSpanDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSpanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
