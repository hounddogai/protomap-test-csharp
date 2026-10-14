namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional succession plan agreement calculation.</summary>
public interface IRegionalSuccessionPlanAgreementModifier
{
    string Describe();
    double Adjust(RegionalSuccessionPlanAgreement item, double valueValue);
}

public sealed record TieredRegionalSuccessionPlanAgreementAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSuccessionPlanAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSuccessionPlanAgreement item, double valueValue)
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

public sealed record MinimumRegionalSuccessionPlanAgreementAdjuster(double Minimum) : IRegionalSuccessionPlanAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSuccessionPlanAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalSuccessionPlanAgreementAdjuster(double Step) : IRegionalSuccessionPlanAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSuccessionPlanAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRegionalSuccessionPlanAgreementAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSuccessionPlanAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSuccessionPlanAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalSuccessionPlanAgreementAdjuster(double Percent) : IRegionalSuccessionPlanAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSuccessionPlanAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional succession plan agreement steps in order and keeps a trace of each.</summary>
public sealed class RegionalSuccessionPlanAgreementEngine(List<IRegionalSuccessionPlanAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSuccessionPlanAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSuccessionPlanAgreementEngine DefaultRegionalSuccessionPlanAgreementEngine(Registry flags)
    {
        return new RegionalSuccessionPlanAgreementEngine(
            new List<IRegionalSuccessionPlanAgreementModifier>
            {
                new TieredRegionalSuccessionPlanAgreementAdjuster(
                    new List<double> { 3204.0, 8712.0 },
                    new List<double> { 1.13, 1.02 }),
                new MinimumRegionalSuccessionPlanAgreementAdjuster(40.1),
                new RoundingRegionalSuccessionPlanAgreementAdjuster(0.05),
                new FlaggedRegionalSuccessionPlanAgreementAdjuster(
                    flags,
                    "regional-succession-plan-agreement-scheduled",
                    1.0),
                new PercentRegionalSuccessionPlanAgreementAdjuster(5.6),
            },
            new List<string>(),
            2.7,
            25211.3);
    }

    public double Price(RegionalSuccessionPlanAgreement item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSuccessionPlanAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
