namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled asset return record calculation.</summary>
public interface IScheduledAssetReturnRecordModifier
{
    string Describe();
    double Adjust(ScheduledAssetReturnRecord item, double valueValue);
}

public sealed record LabelScheduledAssetReturnRecordAdjuster(Dictionary<string, double> Factors) : IScheduledAssetReturnRecordModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledAssetReturnRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledAssetReturnRecordAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAssetReturnRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledAssetReturnRecordVersionOffsetAdjuster(double Weight) : IScheduledAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ScheduledAssetReturnRecord item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record RoundingScheduledAssetReturnRecordAdjuster(double Step) : IScheduledAssetReturnRecordModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAssetReturnRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled asset return record steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAssetReturnRecordEngine(List<IScheduledAssetReturnRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAssetReturnRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAssetReturnRecordEngine DefaultScheduledAssetReturnRecordEngine(Registry flags)
    {
        return new ScheduledAssetReturnRecordEngine(
            new List<IScheduledAssetReturnRecordModifier>
            {
                new LabelScheduledAssetReturnRecordAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.19, ["retail"] = 0.93, ["south"] = 1.17 }),
                new FlaggedScheduledAssetReturnRecordAdjuster(flags, "scheduled-asset-return-record-lenient", -1.4),
                new ScheduledAssetReturnRecordVersionOffsetAdjuster(1.15),
                new RoundingScheduledAssetReturnRecordAdjuster(0.25),
            },
            new List<string>(),
            2.4,
            27104.0);
    }

    public double Evaluate(ScheduledAssetReturnRecord item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAssetReturnRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
