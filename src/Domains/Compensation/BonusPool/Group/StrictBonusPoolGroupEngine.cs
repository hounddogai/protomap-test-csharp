namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict bonus pool group calculation.</summary>
public interface IStrictBonusPoolGroupModifier
{
    string Describe();
    double Adjust(StrictBonusPoolGroup item, double valueValue);
}

public sealed record StrictBonusPoolGroupTenureMonthsOffsetAdjuster(double Weight) : IStrictBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(StrictBonusPoolGroup item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedStrictBonusPoolGroupAdjuster(Registry Flags, string Flag, double Percent) : IStrictBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictBonusPoolGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictBonusPoolGroupAdjuster(List<double> Bounds, List<double> Rates) : IStrictBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictBonusPoolGroup item, double valueValue)
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

public sealed record MinimumStrictBonusPoolGroupAdjuster(double Minimum) : IStrictBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictBonusPoolGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict bonus pool group steps in order and keeps a trace of each.</summary>
public sealed class StrictBonusPoolGroupEngine(List<IStrictBonusPoolGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictBonusPoolGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictBonusPoolGroupEngine DefaultStrictBonusPoolGroupEngine(Registry flags)
    {
        return new StrictBonusPoolGroupEngine(
            new List<IStrictBonusPoolGroupModifier>
            {
                new StrictBonusPoolGroupTenureMonthsOffsetAdjuster(1.07),
                new FlaggedStrictBonusPoolGroupAdjuster(flags, "strict-bonus-pool-group-adaptive", -3.7),
                new TieredStrictBonusPoolGroupAdjuster(
                    new List<double> { 6234.0, 6925.0, 8423.0 },
                    new List<double> { 0.95, 1.04, 1.18 }),
                new MinimumStrictBonusPoolGroupAdjuster(10.4),
            },
            new List<string>(),
            1.9,
            56005.6);
    }

    public double Compute(StrictBonusPoolGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictBonusPoolGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
