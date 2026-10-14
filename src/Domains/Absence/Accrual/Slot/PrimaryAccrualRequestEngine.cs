namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary accrual request calculation.</summary>
public interface IPrimaryAccrualRequestFactor
{
    string Describe();
    double Adjust(PrimaryAccrualRequest item, double valueValue);
}

public sealed record FlaggedPrimaryAccrualRequestAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAccrualRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAccrualRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryAccrualRequestAdjuster(double Step) : IPrimaryAccrualRequestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAccrualRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelPrimaryAccrualRequestAdjuster(Dictionary<string, double> Factors) : IPrimaryAccrualRequestFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PrimaryAccrualRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryAccrualRequestAdjuster(double Minimum) : IPrimaryAccrualRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryAccrualRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPrimaryAccrualRequestAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAccrualRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAccrualRequest item, double valueValue)
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

/// <summary>Runs the primary accrual request steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAccrualRequestEngine(List<IPrimaryAccrualRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAccrualRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAccrualRequestEngine DefaultPrimaryAccrualRequestEngine(Registry flags)
    {
        return new PrimaryAccrualRequestEngine(
            new List<IPrimaryAccrualRequestFactor>
            {
                new FlaggedPrimaryAccrualRequestAdjuster(flags, "primary-accrual-request-primary", 8.4),
                new RoundingPrimaryAccrualRequestAdjuster(0.05),
                new ChannelPrimaryAccrualRequestAdjuster(new Dictionary<string, double> { ["east"] = 0.71 }),
                new MinimumPrimaryAccrualRequestAdjuster(42.0),
                new TieredPrimaryAccrualRequestAdjuster(
                    new List<double> { 2963.0, 7813.0, 9902.0 },
                    new List<double> { 0.85, 0.99, 0.84 }),
            },
            new List<string>(),
            1.3,
            38399.0);
    }

    public double Evaluate(PrimaryAccrualRequest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAccrualRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
