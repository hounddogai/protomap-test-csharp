namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool snapshot calculation.</summary>
public interface IBonusPoolSnapshotModifier
{
    string Describe();
    double Adjust(BonusPoolSnapshot item, double valueValue);
}

public sealed record MinimumBonusPoolSnapshotAdjuster(double Minimum) : IBonusPoolSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BonusPoolSnapshotTenureMonthsOffsetAdjuster(double Weight) : IBonusPoolSnapshotModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(BonusPoolSnapshot item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record CapBonusPoolSnapshotAdjuster(double Cap) : IBonusPoolSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBonusPoolSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool snapshot steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolSnapshotEngine(List<IBonusPoolSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolSnapshotEngine DefaultBonusPoolSnapshotEngine(Registry flags)
    {
        return new BonusPoolSnapshotEngine(
            new List<IBonusPoolSnapshotModifier>
            {
                new MinimumBonusPoolSnapshotAdjuster(6.0),
                new BonusPoolSnapshotTenureMonthsOffsetAdjuster(0.43),
                new CapBonusPoolSnapshotAdjuster(40386.0),
                new FlaggedBonusPoolSnapshotAdjuster(flags, "bonus-pool-snapshot-scheduled", -5.5),
            },
            new List<string>(),
            0.6,
            25581.7);
    }

    public double Evaluate(BonusPoolSnapshot item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
