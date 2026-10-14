namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy span record calculation.</summary>
public interface ILegacySpanRecordFactor
{
    string Describe();
    double Adjust(LegacySpanRecord item, double valueValue);
}

public sealed record CapLegacySpanRecordAdjuster(double Cap) : ILegacySpanRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacySpanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionLegacySpanRecordAdjuster(Dictionary<string, double> Factors) : ILegacySpanRecordFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LegacySpanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacySpanRecordAdjuster(double Step) : ILegacySpanRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacySpanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacySpanRecordAdjuster(Registry Flags, string Flag, double Percent) : ILegacySpanRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacySpanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy span record steps in order and keeps a trace of each.</summary>
public sealed class LegacySpanRecordEngine(List<ILegacySpanRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacySpanRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacySpanRecordEngine DefaultLegacySpanRecordEngine(Registry flags)
    {
        return new LegacySpanRecordEngine(
            new List<ILegacySpanRecordFactor>
            {
                new CapLegacySpanRecordAdjuster(12654.0),
                new RegionLegacySpanRecordAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.13, ["north"] = 0.72, ["west"] = 0.75 }),
                new RoundingLegacySpanRecordAdjuster(0.1),
                new FlaggedLegacySpanRecordAdjuster(flags, "legacy-span-record-legacy", 8.3),
            },
            new List<string>(),
            1.2,
            23916.9);
    }

    public double Compute(LegacySpanRecord item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacySpanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
