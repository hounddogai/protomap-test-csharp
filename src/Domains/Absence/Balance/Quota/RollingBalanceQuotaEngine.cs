namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling balance quota calculation.</summary>
public interface IRollingBalanceQuotaFactor
{
    string Describe();
    double Adjust(RollingBalanceQuota item, double valueValue);
}

public sealed record MinimumRollingBalanceQuotaAdjuster(double Minimum) : IRollingBalanceQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingBalanceQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RollingBalanceQuotaEndsAtOffsetAdjuster(double Weight) : IRollingBalanceQuotaFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(RollingBalanceQuota item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record FlaggedRollingBalanceQuotaAdjuster(Registry Flags, string Flag, double Percent) : IRollingBalanceQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBalanceQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingBalanceQuotaAdjuster(double Step) : IRollingBalanceQuotaFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingBalanceQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling balance quota steps in order and keeps a trace of each.</summary>
public sealed class RollingBalanceQuotaEngine(List<IRollingBalanceQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBalanceQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBalanceQuotaEngine DefaultRollingBalanceQuotaEngine(Registry flags)
    {
        return new RollingBalanceQuotaEngine(
            new List<IRollingBalanceQuotaFactor>
            {
                new MinimumRollingBalanceQuotaAdjuster(44.1),
                new RollingBalanceQuotaEndsAtOffsetAdjuster(0.01),
                new FlaggedRollingBalanceQuotaAdjuster(flags, "rolling-balance-quota-global", 8.5),
                new RoundingRollingBalanceQuotaAdjuster(0.25),
            },
            new List<string>(),
            2.6,
            41624.2);
    }

    public double Evaluate(RollingBalanceQuota item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBalanceQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
