namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg notice calculation.</summary>
public interface IReorgNoticeStep
{
    string Describe();
    double Adjust(ReorgNotice item, double valueValue);
}

public sealed record TieredReorgNoticeAdjuster(List<double> Bounds, List<double> Rates) : IReorgNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgNotice item, double valueValue)
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

public sealed record RoundingReorgNoticeAdjuster(double Step) : IReorgNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReorgNoticeWeightOffsetAdjuster(double Weight) : IReorgNoticeStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ReorgNotice item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedReorgNoticeAdjuster(Registry Flags, string Flag, double Percent) : IReorgNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg notice steps in order and keeps a trace of each.</summary>
public sealed class ReorgNoticeEngine(List<IReorgNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgNoticeEngine DefaultReorgNoticeEngine(Registry flags)
    {
        return new ReorgNoticeEngine(
            new List<IReorgNoticeStep>
            {
                new TieredReorgNoticeAdjuster(
                    new List<double> { 2401.0, 6188.0, 6905.0 },
                    new List<double> { 1.11, 0.83, 0.86 }),
                new RoundingReorgNoticeAdjuster(0.1),
                new ReorgNoticeWeightOffsetAdjuster(1.08),
                new FlaggedReorgNoticeAdjuster(flags, "reorg-notice-manual", -1.3),
            },
            new List<string>(),
            3.5,
            85058.3);
    }

    public double Compute(ReorgNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
