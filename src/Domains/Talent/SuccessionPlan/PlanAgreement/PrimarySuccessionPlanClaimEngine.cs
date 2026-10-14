namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary succession plan claim calculation.</summary>
public interface IPrimarySuccessionPlanClaimFactor
{
    string Describe();
    double Adjust(PrimarySuccessionPlanClaim item, double valueValue);
}

public sealed record PrimarySuccessionPlanClaimFteOffsetAdjuster(double Weight) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredPrimarySuccessionPlanClaimAdjuster(List<double> Bounds, List<double> Rates) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
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

public sealed record RoundingPrimarySuccessionPlanClaimAdjuster(double Step) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocalePrimarySuccessionPlanClaimAdjuster(Dictionary<string, double> Factors) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPrimarySuccessionPlanClaimAdjuster(Registry Flags, string Flag, double Percent) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimarySuccessionPlanClaimAdjuster(double Minimum) : IPrimarySuccessionPlanClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimarySuccessionPlanClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary succession plan claim steps in order and keeps a trace of each.</summary>
public sealed class PrimarySuccessionPlanClaimEngine(List<IPrimarySuccessionPlanClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySuccessionPlanClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySuccessionPlanClaimEngine DefaultPrimarySuccessionPlanClaimEngine(Registry flags)
    {
        return new PrimarySuccessionPlanClaimEngine(
            new List<IPrimarySuccessionPlanClaimFactor>
            {
                new PrimarySuccessionPlanClaimFteOffsetAdjuster(0.52),
                new TieredPrimarySuccessionPlanClaimAdjuster(
                    new List<double> { 7361.0, 8303.0, 9263.0 },
                    new List<double> { 0.96, 0.88, 1.15 }),
                new RoundingPrimarySuccessionPlanClaimAdjuster(0.5),
                new LocalePrimarySuccessionPlanClaimAdjuster(new Dictionary<string, double> { ["west"] = 0.71 }),
                new FlaggedPrimarySuccessionPlanClaimAdjuster(flags, "primary-succession-plan-claim-pending", -9.7),
                new MinimumPrimarySuccessionPlanClaimAdjuster(38.2),
            },
            new List<string>(),
            3.2,
            68634.5);
    }

    public double Run(PrimarySuccessionPlanClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySuccessionPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
