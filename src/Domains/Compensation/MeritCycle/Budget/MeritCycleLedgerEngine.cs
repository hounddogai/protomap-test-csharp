namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle ledger calculation.</summary>
public interface IMeritCycleLedgerAdjuster
{
    string Describe();
    double Adjust(MeritCycleLedger item, double valueValue);
}

public sealed record RoundingMeritCycleLedgerAdjuster(double Step) : IMeritCycleLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedMeritCycleLedgerAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MeritCycleLedgerLimitOffsetAdjuster(double Weight) : IMeritCycleLedgerAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(MeritCycleLedger item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredMeritCycleLedgerAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleLedger item, double valueValue)
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

/// <summary>Runs the merit cycle ledger steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleLedgerEngine(List<IMeritCycleLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleLedgerEngine DefaultMeritCycleLedgerEngine(Registry flags)
    {
        return new MeritCycleLedgerEngine(
            new List<IMeritCycleLedgerAdjuster>
            {
                new RoundingMeritCycleLedgerAdjuster(0.5),
                new FlaggedMeritCycleLedgerAdjuster(flags, "merit-cycle-ledger-manual", 4.7),
                new MeritCycleLedgerLimitOffsetAdjuster(0.75),
                new TieredMeritCycleLedgerAdjuster(
                    new List<double> { 1467.0, 3073.0, 4345.0, 7042.0 },
                    new List<double> { 0.83, 0.91, 0.93, 0.83 }),
            },
            new List<string>(),
            1.7,
            94059.8);
    }

    public double Run(MeritCycleLedger item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
