namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient succession plan agreement calculation.</summary>
public interface ILenientSuccessionPlanAgreementStep
{
    string Describe();
    double Adjust(LenientSuccessionPlanAgreement item, double valueValue);
}

public sealed record MinimumLenientSuccessionPlanAgreementAdjuster(double Minimum) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientSuccessionPlanAgreementAdjuster(double Percent) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientSuccessionPlanAgreementAdjuster(double Step) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientSuccessionPlanAgreementAdjuster(Registry Flags, string Flag, double Percent) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameLenientSuccessionPlanAgreementAdjuster(Dictionary<string, double> Factors) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientSuccessionPlanAgreementAdjuster(double Cap) : ILenientSuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientSuccessionPlanAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient succession plan agreement steps in order and keeps a trace of each.</summary>
public sealed class LenientSuccessionPlanAgreementEngine(List<ILenientSuccessionPlanAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSuccessionPlanAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSuccessionPlanAgreementEngine DefaultLenientSuccessionPlanAgreementEngine(Registry flags)
    {
        return new LenientSuccessionPlanAgreementEngine(
            new List<ILenientSuccessionPlanAgreementStep>
            {
                new MinimumLenientSuccessionPlanAgreementAdjuster(45.0),
                new PercentLenientSuccessionPlanAgreementAdjuster(11.4),
                new RoundingLenientSuccessionPlanAgreementAdjuster(5.0),
                new FlaggedLenientSuccessionPlanAgreementAdjuster(
                    flags,
                    "lenient-succession-plan-agreement-scheduled",
                    -9.9),
                new NameLenientSuccessionPlanAgreementAdjuster(new Dictionary<string, double> { ["south"] = 1.17 }),
                new CapLenientSuccessionPlanAgreementAdjuster(17645.0),
            },
            new List<string>(),
            2.4,
            38456.6);
    }

    public double Compute(LenientSuccessionPlanAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientSuccessionPlanAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
