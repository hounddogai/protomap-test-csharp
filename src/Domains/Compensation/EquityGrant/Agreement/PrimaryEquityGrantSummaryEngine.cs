namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary equity grant summary calculation.</summary>
public interface IPrimaryEquityGrantSummaryAdjuster
{
    string Describe();
    double Adjust(PrimaryEquityGrantSummary item, double valueValue);
}

public sealed record FlaggedPrimaryEquityGrantSummaryAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryEquityGrantSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryEquityGrantSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryEquityGrantSummaryTenureMonthsOffsetAdjuster(double Weight) : IPrimaryEquityGrantSummaryAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PrimaryEquityGrantSummary item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TieredPrimaryEquityGrantSummaryAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryEquityGrantSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryEquityGrantSummary item, double valueValue)
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

/// <summary>Runs the primary equity grant summary steps in order and keeps a trace of each.</summary>
public sealed class PrimaryEquityGrantSummaryEngine(List<IPrimaryEquityGrantSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryEquityGrantSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryEquityGrantSummaryEngine DefaultPrimaryEquityGrantSummaryEngine(Registry flags)
    {
        return new PrimaryEquityGrantSummaryEngine(
            new List<IPrimaryEquityGrantSummaryAdjuster>
            {
                new FlaggedPrimaryEquityGrantSummaryAdjuster(flags, "primary-equity-grant-summary-default", -1.3),
                new PrimaryEquityGrantSummaryTenureMonthsOffsetAdjuster(0.81),
                new TieredPrimaryEquityGrantSummaryAdjuster(
                    new List<double> { 5028.0, 5098.0, 5771.0, 8701.0 },
                    new List<double> { 1.16, 1.04, 1.17, 1.2 }),
            },
            new List<string>(),
            0.5,
            15881.6);
    }

    public double Price(PrimaryEquityGrantSummary item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryEquityGrantSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
