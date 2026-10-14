namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy position ledger calculation.</summary>
public interface ILegacyPositionLedgerStep
{
    string Describe();
    double Adjust(LegacyPositionLedger item, double valueValue);
}

public sealed record FlaggedLegacyPositionLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyPositionLedgerAdjuster(double Minimum) : ILegacyPositionLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPositionLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLegacyPositionLedgerAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPositionLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPositionLedger item, double valueValue)
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

/// <summary>Runs the legacy position ledger steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionLedgerEngine(List<ILegacyPositionLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionLedgerEngine DefaultLegacyPositionLedgerEngine(Registry flags)
    {
        return new LegacyPositionLedgerEngine(
            new List<ILegacyPositionLedgerStep>
            {
                new FlaggedLegacyPositionLedgerAdjuster(flags, "legacy-position-ledger-legacy", 2.3),
                new MinimumLegacyPositionLedgerAdjuster(37.1),
                new TieredLegacyPositionLedgerAdjuster(
                    new List<double> { 2217.0, 4349.0 },
                    new List<double> { 0.91, 0.94 }),
            },
            new List<string>(),
            1.0,
            46713.9);
    }

    public double Compute(LegacyPositionLedger item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
