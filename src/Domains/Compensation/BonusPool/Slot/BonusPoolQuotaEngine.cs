namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool quota calculation.</summary>
public interface IBonusPoolQuotaAdjuster
{
    string Describe();
    double Adjust(BonusPoolQuota item, double valueValue);
}

public sealed record RoundingBonusPoolQuotaAdjuster(double Step) : IBonusPoolQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBonusPoolQuotaAdjuster(double Minimum) : IBonusPoolQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBonusPoolQuotaAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBonusPoolQuotaAdjuster(double Cap) : IBonusPoolQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BonusPoolQuotaExpiresAtOffsetAdjuster(double Weight) : IBonusPoolQuotaAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(BonusPoolQuota item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the bonus pool quota steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolQuotaEngine(List<IBonusPoolQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolQuotaEngine DefaultBonusPoolQuotaEngine(Registry flags)
    {
        return new BonusPoolQuotaEngine(
            new List<IBonusPoolQuotaAdjuster>
            {
                new RoundingBonusPoolQuotaAdjuster(0.1),
                new MinimumBonusPoolQuotaAdjuster(43.6),
                new FlaggedBonusPoolQuotaAdjuster(flags, "bonus-pool-quota-batched", -7.3),
                new CapBonusPoolQuotaAdjuster(2680.0),
                new BonusPoolQuotaExpiresAtOffsetAdjuster(1.37),
            },
            new List<string>(),
            0.3,
            83590.7);
    }

    public double Price(BonusPoolQuota item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
