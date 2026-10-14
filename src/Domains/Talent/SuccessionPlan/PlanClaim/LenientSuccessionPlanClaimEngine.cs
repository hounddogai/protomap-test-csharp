namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient succession plan claim calculation.</summary>
public interface ILenientSuccessionPlanClaimModifier
{
    string Describe();
    double Adjust(LenientSuccessionPlanClaim item, double valueValue);
}

public sealed record MinimumLenientSuccessionPlanClaimAdjuster(double Minimum) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientSuccessionPlanClaimAdjuster(double Percent) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientSuccessionPlanClaimAdjuster(Registry Flags, string Flag, double Percent) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientSuccessionPlanClaimAdjuster(double Step) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientSuccessionPlanClaimAdjuster(double Cap) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientSuccessionPlanClaimAdjuster(List<double> Bounds, List<double> Rates) : ILenientSuccessionPlanClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientSuccessionPlanClaim item, double valueValue)
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

/// <summary>Runs the lenient succession plan claim steps in order and keeps a trace of each.</summary>
public sealed class LenientSuccessionPlanClaimEngine(List<ILenientSuccessionPlanClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSuccessionPlanClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSuccessionPlanClaimEngine DefaultLenientSuccessionPlanClaimEngine(Registry flags)
    {
        return new LenientSuccessionPlanClaimEngine(
            new List<ILenientSuccessionPlanClaimModifier>
            {
                new MinimumLenientSuccessionPlanClaimAdjuster(18.1),
                new PercentLenientSuccessionPlanClaimAdjuster(12.7),
                new FlaggedLenientSuccessionPlanClaimAdjuster(flags, "lenient-succession-plan-claim-default", -7.6),
                new RoundingLenientSuccessionPlanClaimAdjuster(0.25),
                new CapLenientSuccessionPlanClaimAdjuster(26751.0),
                new TieredLenientSuccessionPlanClaimAdjuster(
                    new List<double> { 2941.0, 9226.0 },
                    new List<double> { 1.05, 1.12 }),
            },
            new List<string>(),
            2.6,
            43922.3);
    }

    public double Evaluate(LenientSuccessionPlanClaim item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientSuccessionPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
