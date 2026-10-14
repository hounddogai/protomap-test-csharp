namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered garnishment quota calculation.</summary>
public interface ITieredGarnishmentQuotaAdjuster
{
    string Describe();
    double Adjust(TieredGarnishmentQuota item, double valueValue);
}

public sealed record RoundingTieredGarnishmentQuotaAdjuster(double Step) : ITieredGarnishmentQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredGarnishmentQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredGarnishmentQuotaAdjuster(double Minimum) : ITieredGarnishmentQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredGarnishmentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTieredGarnishmentQuotaAdjuster(double Cap) : ITieredGarnishmentQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredGarnishmentQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionTieredGarnishmentQuotaAdjuster(Dictionary<string, double> Factors) : ITieredGarnishmentQuotaAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TieredGarnishmentQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredGarnishmentQuotaAdjuster(Registry Flags, string Flag, double Percent) : ITieredGarnishmentQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredGarnishmentQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered garnishment quota steps in order and keeps a trace of each.</summary>
public sealed class TieredGarnishmentQuotaEngine(List<ITieredGarnishmentQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredGarnishmentQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredGarnishmentQuotaEngine DefaultTieredGarnishmentQuotaEngine(Registry flags)
    {
        return new TieredGarnishmentQuotaEngine(
            new List<ITieredGarnishmentQuotaAdjuster>
            {
                new RoundingTieredGarnishmentQuotaAdjuster(0.1),
                new MinimumTieredGarnishmentQuotaAdjuster(20.1),
                new CapTieredGarnishmentQuotaAdjuster(12480.0),
                new RegionTieredGarnishmentQuotaAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.3, ["west"] = 0.96, ["north"] = 0.95 }),
                new FlaggedTieredGarnishmentQuotaAdjuster(flags, "tiered-garnishment-quota-seasonal", 1.5),
            },
            new List<string>(),
            0.2,
            1570.8);
    }

    public double Compute(TieredGarnishmentQuota item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredGarnishmentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
