namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool ledger calculation.</summary>
public interface IBonusPoolLedgerModifier
{
    string Describe();
    double Adjust(BonusPoolLedger item, double valueValue);
}

public sealed record MinimumBonusPoolLedgerAdjuster(double Minimum) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentBonusPoolLedgerAdjuster(Dictionary<string, double> Factors) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBonusPoolLedgerAdjuster(double Cap) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBonusPoolLedgerAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBonusPoolLedgerAdjuster(double Step) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBonusPoolLedgerAdjuster(double Percent) : IBonusPoolLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool ledger steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolLedgerEngine(List<IBonusPoolLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolLedgerEngine DefaultBonusPoolLedgerEngine(Registry flags)
    {
        return new BonusPoolLedgerEngine(
            new List<IBonusPoolLedgerModifier>
            {
                new MinimumBonusPoolLedgerAdjuster(19.6),
                new SegmentBonusPoolLedgerAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.95, ["north"] = 0.87, ["online"] = 0.71 }),
                new CapBonusPoolLedgerAdjuster(28069.0),
                new FlaggedBonusPoolLedgerAdjuster(flags, "bonus-pool-ledger-weighted", -8.7),
                new RoundingBonusPoolLedgerAdjuster(0.5),
                new PercentBonusPoolLedgerAdjuster(18.8),
            },
            new List<string>(),
            0.2,
            66889.0);
    }

    public double Price(BonusPoolLedger item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
