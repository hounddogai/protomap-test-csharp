namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional laptop ledger calculation.</summary>
public interface IRegionalLaptopLedgerModifier
{
    string Describe();
    double Adjust(RegionalLaptopLedger item, double valueValue);
}

public sealed record TieredRegionalLaptopLedgerAdjuster(List<double> Bounds, List<double> Rates) : IRegionalLaptopLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalLaptopLedger item, double valueValue)
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

public sealed record RoundingRegionalLaptopLedgerAdjuster(double Step) : IRegionalLaptopLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalLaptopLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRegionalLaptopLedgerAdjuster(Registry Flags, string Flag, double Percent) : IRegionalLaptopLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalLaptopLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional laptop ledger steps in order and keeps a trace of each.</summary>
public sealed class RegionalLaptopLedgerEngine(List<IRegionalLaptopLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalLaptopLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalLaptopLedgerEngine DefaultRegionalLaptopLedgerEngine(Registry flags)
    {
        return new RegionalLaptopLedgerEngine(
            new List<IRegionalLaptopLedgerModifier>
            {
                new TieredRegionalLaptopLedgerAdjuster(
                    new List<double> { 925.0, 2359.0, 4689.0, 7450.0 },
                    new List<double> { 0.9, 0.97, 1.18, 1.02 }),
                new RoundingRegionalLaptopLedgerAdjuster(0.05),
                new FlaggedRegionalLaptopLedgerAdjuster(flags, "regional-laptop-ledger-legacy", 3.6),
            },
            new List<string>(),
            0.4,
            73591.9);
    }

    public double Compute(RegionalLaptopLedger item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalLaptopLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
