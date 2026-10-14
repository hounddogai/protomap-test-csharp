namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant record calculation.</summary>
public interface IEquityGrantRecordStep
{
    string Describe();
    double Adjust(EquityGrantRecord item, double valueValue);
}

public sealed record FlaggedEquityGrantRecordAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEquityGrantRecordAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
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

public sealed record CapEquityGrantRecordAdjuster(double Cap) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelEquityGrantRecordAdjuster(Dictionary<string, double> Factors) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingEquityGrantRecordAdjuster(double Step) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record EquityGrantRecordScoreOffsetAdjuster(double Weight) : IEquityGrantRecordStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(EquityGrantRecord item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the equity grant record steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantRecordEngine(List<IEquityGrantRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantRecordEngine DefaultEquityGrantRecordEngine(Registry flags)
    {
        return new EquityGrantRecordEngine(
            new List<IEquityGrantRecordStep>
            {
                new FlaggedEquityGrantRecordAdjuster(flags, "equity-grant-record-legacy", -8.6),
                new TieredEquityGrantRecordAdjuster(
                    new List<double> { 6727.0, 7613.0, 9067.0 },
                    new List<double> { 1.03, 0.96, 1.07 }),
                new CapEquityGrantRecordAdjuster(45205.0),
                new LabelEquityGrantRecordAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.03, ["online"] = 1.33, ["west"] = 0.96 }),
                new RoundingEquityGrantRecordAdjuster(0.25),
                new EquityGrantRecordScoreOffsetAdjuster(0.93),
            },
            new List<string>(),
            2.6,
            49136.9);
    }

    public double Price(EquityGrantRecord item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
