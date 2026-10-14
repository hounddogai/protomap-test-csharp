namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled balance agreement calculation.</summary>
public interface IScheduledBalanceAgreementModifier
{
    string Describe();
    double Adjust(ScheduledBalanceAgreement item, double valueValue);
}

public sealed record SourceScheduledBalanceAgreementAdjuster(Dictionary<string, double> Factors) : IScheduledBalanceAgreementModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ScheduledBalanceAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledBalanceAgreementGraceDaysOffsetAdjuster(double Weight) : IScheduledBalanceAgreementModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ScheduledBalanceAgreement item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record FlaggedScheduledBalanceAgreementAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBalanceAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBalanceAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledBalanceAgreementAdjuster(List<double> Bounds, List<double> Rates) : IScheduledBalanceAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledBalanceAgreement item, double valueValue)
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

public sealed record RoundingScheduledBalanceAgreementAdjuster(double Step) : IScheduledBalanceAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBalanceAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled balance agreement steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBalanceAgreementEngine(List<IScheduledBalanceAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBalanceAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBalanceAgreementEngine DefaultScheduledBalanceAgreementEngine(Registry flags)
    {
        return new ScheduledBalanceAgreementEngine(
            new List<IScheduledBalanceAgreementModifier>
            {
                new SourceScheduledBalanceAgreementAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.18, ["online"] = 0.72, ["retail"] = 1.28 }),
                new ScheduledBalanceAgreementGraceDaysOffsetAdjuster(0.79),
                new FlaggedScheduledBalanceAgreementAdjuster(flags, "scheduled-balance-agreement-weighted", -1.4),
                new TieredScheduledBalanceAgreementAdjuster(
                    new List<double> { 9325.0, 9589.0 },
                    new List<double> { 0.84, 0.94 }),
                new RoundingScheduledBalanceAgreementAdjuster(0.05),
            },
            new List<string>(),
            4.8,
            10786.2);
    }

    public double Price(ScheduledBalanceAgreement item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBalanceAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
