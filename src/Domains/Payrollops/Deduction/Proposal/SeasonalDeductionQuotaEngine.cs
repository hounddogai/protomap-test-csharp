namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal deduction quota calculation.</summary>
public interface ISeasonalDeductionQuotaAdjuster
{
    string Describe();
    double Adjust(SeasonalDeductionQuota item, double valueValue);
}

public sealed record PercentSeasonalDeductionQuotaAdjuster(double Percent) : ISeasonalDeductionQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalDeductionQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSeasonalDeductionQuotaAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalDeductionQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalDeductionQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelSeasonalDeductionQuotaAdjuster(Dictionary<string, double> Factors) : ISeasonalDeductionQuotaAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(SeasonalDeductionQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalDeductionQuotaAdjuster(double Cap) : ISeasonalDeductionQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalDeductionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalDeductionQuotaAdjuster(double Step) : ISeasonalDeductionQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalDeductionQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal deduction quota steps in order and keeps a trace of each.</summary>
public sealed class SeasonalDeductionQuotaEngine(List<ISeasonalDeductionQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalDeductionQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalDeductionQuotaEngine DefaultSeasonalDeductionQuotaEngine(Registry flags)
    {
        return new SeasonalDeductionQuotaEngine(
            new List<ISeasonalDeductionQuotaAdjuster>
            {
                new PercentSeasonalDeductionQuotaAdjuster(1.6),
                new FlaggedSeasonalDeductionQuotaAdjuster(flags, "seasonal-deduction-quota-weighted", 0.7),
                new ChannelSeasonalDeductionQuotaAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.84, ["east"] = 1.16 }),
                new CapSeasonalDeductionQuotaAdjuster(46072.0),
                new RoundingSeasonalDeductionQuotaAdjuster(1.0),
            },
            new List<string>(),
            3.1,
            32635.9);
    }

    public double Evaluate(SeasonalDeductionQuota item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalDeductionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
