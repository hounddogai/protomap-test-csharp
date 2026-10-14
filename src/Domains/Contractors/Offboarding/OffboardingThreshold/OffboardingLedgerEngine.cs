namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding ledger calculation.</summary>
public interface IOffboardingLedgerModifier
{
    string Describe();
    double Adjust(OffboardingLedger item, double valueValue);
}

public sealed record TieredOffboardingLedgerAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingLedger item, double valueValue)
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

public sealed record MinimumOffboardingLedgerAdjuster(double Minimum) : IOffboardingLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record OffboardingLedgerMinimumOffsetAdjuster(double Weight) : IOffboardingLedgerModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(OffboardingLedger item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedOffboardingLedgerAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentOffboardingLedgerAdjuster(double Percent) : IOffboardingLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding ledger steps in order and keeps a trace of each.</summary>
public sealed class OffboardingLedgerEngine(List<IOffboardingLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingLedgerEngine DefaultOffboardingLedgerEngine(Registry flags)
    {
        return new OffboardingLedgerEngine(
            new List<IOffboardingLedgerModifier>
            {
                new TieredOffboardingLedgerAdjuster(
                    new List<double> { 1426.0, 7106.0, 9289.0 },
                    new List<double> { 0.99, 0.84, 0.96 }),
                new MinimumOffboardingLedgerAdjuster(23.4),
                new OffboardingLedgerMinimumOffsetAdjuster(1.14),
                new FlaggedOffboardingLedgerAdjuster(flags, "offboarding-ledger-tiered", -10.0),
                new PercentOffboardingLedgerAdjuster(13.8),
            },
            new List<string>(),
            0.0,
            68852.1);
    }

    public double Run(OffboardingLedger item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
