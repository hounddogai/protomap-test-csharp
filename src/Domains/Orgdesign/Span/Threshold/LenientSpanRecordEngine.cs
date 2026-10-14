namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient span record calculation.</summary>
public interface ILenientSpanRecordFactor
{
    string Describe();
    double Adjust(LenientSpanRecord item, double valueValue);
}

public sealed record RoundingLenientSpanRecordAdjuster(double Step) : ILenientSpanRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSpanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceLenientSpanRecordAdjuster(Dictionary<string, double> Factors) : ILenientSpanRecordFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientSpanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientSpanRecordAdjuster(double Cap) : ILenientSpanRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientSpanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLenientSpanRecordAdjuster(Registry Flags, string Flag, double Percent) : ILenientSpanRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSpanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientSpanRecordAmountOffsetAdjuster(double Weight) : ILenientSpanRecordFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientSpanRecord item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the lenient span record steps in order and keeps a trace of each.</summary>
public sealed class LenientSpanRecordEngine(List<ILenientSpanRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSpanRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSpanRecordEngine DefaultLenientSpanRecordEngine(Registry flags)
    {
        return new LenientSpanRecordEngine(
            new List<ILenientSpanRecordFactor>
            {
                new RoundingLenientSpanRecordAdjuster(0.05),
                new SourceLenientSpanRecordAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["online"] = 1.23 }),
                new CapLenientSpanRecordAdjuster(4138.0),
                new FlaggedLenientSpanRecordAdjuster(flags, "lenient-span-record-strict", -2.3),
                new LenientSpanRecordAmountOffsetAdjuster(0.75),
            },
            new List<string>(),
            4.8,
            52499.6);
    }

    public double Price(LenientSpanRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientSpanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
