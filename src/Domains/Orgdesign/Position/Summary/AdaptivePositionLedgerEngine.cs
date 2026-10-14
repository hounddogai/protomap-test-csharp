namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive position ledger calculation.</summary>
public interface IAdaptivePositionLedgerFactor
{
    string Describe();
    double Adjust(AdaptivePositionLedger item, double valueValue);
}

public sealed record CapAdaptivePositionLedgerAdjuster(double Cap) : IAdaptivePositionLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePositionLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptivePositionLedgerAdjuster(double Minimum) : IAdaptivePositionLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePositionLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptivePositionLedgerAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePositionLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePositionLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive position ledger steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePositionLedgerEngine(List<IAdaptivePositionLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePositionLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePositionLedgerEngine DefaultAdaptivePositionLedgerEngine(Registry flags)
    {
        return new AdaptivePositionLedgerEngine(
            new List<IAdaptivePositionLedgerFactor>
            {
                new CapAdaptivePositionLedgerAdjuster(47602.0),
                new MinimumAdaptivePositionLedgerAdjuster(25.6),
                new FlaggedAdaptivePositionLedgerAdjuster(flags, "adaptive-position-ledger-primary", 8.4),
            },
            new List<string>(),
            4.6,
            64166.1);
    }

    public double Evaluate(AdaptivePositionLedger item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePositionLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
