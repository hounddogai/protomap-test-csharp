namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback access badge agreement calculation.</summary>
public interface IFallbackAccessBadgeAgreementModifier
{
    string Describe();
    double Adjust(FallbackAccessBadgeAgreement item, double valueValue);
}

public sealed record MinimumFallbackAccessBadgeAgreementAdjuster(double Minimum) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFallbackAccessBadgeAgreementAdjuster(double Step) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackAccessBadgeAgreementAdjuster(double Percent) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackAccessBadgeAgreementMarginOffsetAdjuster(double Weight) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapFallbackAccessBadgeAgreementAdjuster(double Cap) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFallbackAccessBadgeAgreementAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAccessBadgeAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAccessBadgeAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback access badge agreement steps in order and keeps a trace of each.</summary>
public sealed class FallbackAccessBadgeAgreementEngine(List<IFallbackAccessBadgeAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAccessBadgeAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAccessBadgeAgreementEngine DefaultFallbackAccessBadgeAgreementEngine(Registry flags)
    {
        return new FallbackAccessBadgeAgreementEngine(
            new List<IFallbackAccessBadgeAgreementModifier>
            {
                new MinimumFallbackAccessBadgeAgreementAdjuster(14.8),
                new RoundingFallbackAccessBadgeAgreementAdjuster(0.5),
                new PercentFallbackAccessBadgeAgreementAdjuster(-22.9),
                new FallbackAccessBadgeAgreementMarginOffsetAdjuster(1.33),
                new CapFallbackAccessBadgeAgreementAdjuster(22520.0),
                new FlaggedFallbackAccessBadgeAgreementAdjuster(
                    flags,
                    "fallback-access-badge-agreement-legacy",
                    -9.6),
            },
            new List<string>(),
            4.2,
            74225.7);
    }

    public double Run(FallbackAccessBadgeAgreement item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAccessBadgeAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
