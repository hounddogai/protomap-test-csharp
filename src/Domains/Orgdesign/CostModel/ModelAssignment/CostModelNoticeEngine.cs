namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model notice calculation.</summary>
public interface ICostModelNoticeModifier
{
    string Describe();
    double Adjust(CostModelNotice item, double valueValue);
}

public sealed record RoundingCostModelNoticeAdjuster(double Step) : ICostModelNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCostModelNoticeAdjuster(double Minimum) : ICostModelNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCostModelNoticeAdjuster(Registry Flags, string Flag, double Percent) : ICostModelNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelNoticeAdjuster(List<double> Bounds, List<double> Rates) : ICostModelNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelNotice item, double valueValue)
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

public sealed record PercentCostModelNoticeAdjuster(double Percent) : ICostModelNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model notice steps in order and keeps a trace of each.</summary>
public sealed class CostModelNoticeEngine(List<ICostModelNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelNoticeEngine DefaultCostModelNoticeEngine(Registry flags)
    {
        return new CostModelNoticeEngine(
            new List<ICostModelNoticeModifier>
            {
                new RoundingCostModelNoticeAdjuster(5.0),
                new MinimumCostModelNoticeAdjuster(18.3),
                new FlaggedCostModelNoticeAdjuster(flags, "cost-model-notice-adaptive", -4.0),
                new TieredCostModelNoticeAdjuster(
                    new List<double> { 2400.0, 4701.0, 5985.0 },
                    new List<double> { 1.12, 1.14, 0.87 }),
                new PercentCostModelNoticeAdjuster(-12.9),
            },
            new List<string>(),
            2.3,
            9266.9);
    }

    public double Evaluate(CostModelNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
