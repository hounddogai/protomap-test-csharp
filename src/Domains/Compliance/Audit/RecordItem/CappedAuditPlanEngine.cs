namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped audit plan calculation.</summary>
public interface ICappedAuditPlanStep
{
    string Describe();
    double Adjust(CappedAuditPlan item, double valueValue);
}

public sealed record FlaggedCappedAuditPlanAdjuster(Registry Flags, string Flag, double Percent) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedAuditPlanMinimumOffsetAdjuster(double Weight) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record UnitCappedAuditPlanAdjuster(Dictionary<string, double> Factors) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedAuditPlanAdjuster(List<double> Bounds, List<double> Rates) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
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

public sealed record PercentCappedAuditPlanAdjuster(double Percent) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedAuditPlanAdjuster(double Step) : ICappedAuditPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAuditPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped audit plan steps in order and keeps a trace of each.</summary>
public sealed class CappedAuditPlanEngine(List<ICappedAuditPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAuditPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAuditPlanEngine DefaultCappedAuditPlanEngine(Registry flags)
    {
        return new CappedAuditPlanEngine(
            new List<ICappedAuditPlanStep>
            {
                new FlaggedCappedAuditPlanAdjuster(flags, "capped-audit-plan-global", -8.6),
                new CappedAuditPlanMinimumOffsetAdjuster(1.52),
                new UnitCappedAuditPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["online"] = 1.14 }),
                new TieredCappedAuditPlanAdjuster(
                    new List<double> { 321.0, 7196.0, 8040.0, 8733.0 },
                    new List<double> { 0.95, 0.83, 0.84, 0.97 }),
                new PercentCappedAuditPlanAdjuster(-21.9),
                new RoundingCappedAuditPlanAdjuster(1.0),
            },
            new List<string>(),
            2.0,
            38985.1);
    }

    public double Price(CappedAuditPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedAuditPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
