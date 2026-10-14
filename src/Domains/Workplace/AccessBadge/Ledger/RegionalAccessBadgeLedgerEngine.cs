namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional access badge ledger calculation.</summary>
public interface IRegionalAccessBadgeLedgerModifier
{
    string Describe();
    double Adjust(RegionalAccessBadgeLedger item, double valueValue);
}

public sealed record TieredRegionalAccessBadgeLedgerAdjuster(List<double> Bounds, List<double> Rates) : IRegionalAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalAccessBadgeLedger item, double valueValue)
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

public sealed record RegionalAccessBadgeLedgerCapacityOffsetAdjuster(double Weight) : IRegionalAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(RegionalAccessBadgeLedger item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedRegionalAccessBadgeLedgerAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAccessBadgeLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalAccessBadgeLedgerAdjuster(double Percent) : IRegionalAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAccessBadgeLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalAccessBadgeLedgerAdjuster(double Step) : IRegionalAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalAccessBadgeLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional access badge ledger steps in order and keeps a trace of each.</summary>
public sealed class RegionalAccessBadgeLedgerEngine(List<IRegionalAccessBadgeLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAccessBadgeLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAccessBadgeLedgerEngine DefaultRegionalAccessBadgeLedgerEngine(Registry flags)
    {
        return new RegionalAccessBadgeLedgerEngine(
            new List<IRegionalAccessBadgeLedgerModifier>
            {
                new TieredRegionalAccessBadgeLedgerAdjuster(
                    new List<double> { 3969.0, 9602.0 },
                    new List<double> { 1.0, 0.95 }),
                new RegionalAccessBadgeLedgerCapacityOffsetAdjuster(1.84),
                new FlaggedRegionalAccessBadgeLedgerAdjuster(flags, "regional-access-badge-ledger-tiered", -0.9),
                new PercentRegionalAccessBadgeLedgerAdjuster(-23.4),
                new RoundingRegionalAccessBadgeLedgerAdjuster(0.25),
            },
            new List<string>(),
            3.9,
            37607.0);
    }

    public double Compute(RegionalAccessBadgeLedger item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAccessBadgeLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
