namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility ledger calculation.</summary>
public interface IMobilityLedgerModifier
{
    string Describe();
    double Adjust(MobilityLedger item, double valueValue);
}

public sealed record FlaggedMobilityLedgerAdjuster(Registry Flags, string Flag, double Percent) : IMobilityLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMobilityLedgerAdjuster(double Minimum) : IMobilityLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredMobilityLedgerAdjuster(List<double> Bounds, List<double> Rates) : IMobilityLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityLedger item, double valueValue)
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

/// <summary>Runs the mobility ledger steps in order and keeps a trace of each.</summary>
public sealed class MobilityLedgerEngine(List<IMobilityLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityLedgerEngine DefaultMobilityLedgerEngine(Registry flags)
    {
        return new MobilityLedgerEngine(
            new List<IMobilityLedgerModifier>
            {
                new FlaggedMobilityLedgerAdjuster(flags, "mobility-ledger-seasonal", -7.9),
                new MinimumMobilityLedgerAdjuster(34.7),
                new TieredMobilityLedgerAdjuster(
                    new List<double> { 925.0, 2389.0, 4189.0 },
                    new List<double> { 1.17, 1.02, 1.01 }),
            },
            new List<string>(),
            0.8,
            64770.6);
    }

    public double Run(MobilityLedger item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
