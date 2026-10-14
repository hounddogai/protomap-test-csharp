namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the reorg summary calculation.</summary>
public interface IReorgSummaryStep
{
    string Describe();
    double Adjust(ReorgSummary item, double valueValue);
}

public sealed record FlaggedReorgSummaryAdjuster(Registry Flags, string Flag, double Percent) : IReorgSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ReorgSummaryMarginOffsetAdjuster(double Weight) : IReorgSummaryStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ReorgSummary item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredReorgSummaryAdjuster(List<double> Bounds, List<double> Rates) : IReorgSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgSummary item, double valueValue)
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

/// <summary>Runs the reorg summary steps in order and keeps a trace of each.</summary>
public sealed class ReorgSummaryEngine(List<IReorgSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgSummaryEngine DefaultReorgSummaryEngine(Registry flags)
    {
        return new ReorgSummaryEngine(
            new List<IReorgSummaryStep>
            {
                new FlaggedReorgSummaryAdjuster(flags, "reorg-summary-primary", 8.5),
                new ReorgSummaryMarginOffsetAdjuster(0.1),
                new TieredReorgSummaryAdjuster(
                    new List<double> { 3467.0, 3537.0, 5528.0, 8191.0 },
                    new List<double> { 0.8, 0.83, 0.85, 0.93 }),
            },
            new List<string>(),
            3.2,
            87468.2);
    }

    public double Price(ReorgSummary item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
