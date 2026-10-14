namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant quota calculation.</summary>
public interface IEquityGrantQuotaModifier
{
    string Describe();
    double Adjust(EquityGrantQuota item, double valueValue);
}

public sealed record FlaggedEquityGrantQuotaAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EquityGrantQuotaUpdatedAtOffsetAdjuster(double Weight) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredEquityGrantQuotaAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
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

public sealed record UnitEquityGrantQuotaAdjuster(Dictionary<string, double> Factors) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentEquityGrantQuotaAdjuster(double Percent) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantQuotaAdjuster(double Step) : IEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant quota steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantQuotaEngine(List<IEquityGrantQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantQuotaEngine DefaultEquityGrantQuotaEngine(Registry flags)
    {
        return new EquityGrantQuotaEngine(
            new List<IEquityGrantQuotaModifier>
            {
                new FlaggedEquityGrantQuotaAdjuster(flags, "equity-grant-quota-rolling", -0.9),
                new EquityGrantQuotaUpdatedAtOffsetAdjuster(1.43),
                new TieredEquityGrantQuotaAdjuster(
                    new List<double> { 3972.0, 7063.0 },
                    new List<double> { 1.08, 1.07 }),
                new UnitEquityGrantQuotaAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.21, ["central"] = 0.73, ["retail"] = 1.27 }),
                new PercentEquityGrantQuotaAdjuster(-23.5),
                new RoundingEquityGrantQuotaAdjuster(5.0),
            },
            new List<string>(),
            0.9,
            11389.6);
    }

    public double Evaluate(EquityGrantQuota item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
