namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay band agreement calculation.</summary>
public interface ILegacyPayBandAgreementFactor
{
    string Describe();
    double Adjust(LegacyPayBandAgreement item, double valueValue);
}

public sealed record TieredLegacyPayBandAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
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

public sealed record CountryLegacyPayBandAgreementAdjuster(Dictionary<string, double> Factors) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyPayBandAgreementAdjuster(double Minimum) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyPayBandAgreementAdjuster(double Step) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyPayBandAgreementAdjuster(double Percent) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyPayBandAgreementAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPayBandAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPayBandAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy pay band agreement steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayBandAgreementEngine(List<ILegacyPayBandAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayBandAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayBandAgreementEngine DefaultLegacyPayBandAgreementEngine(Registry flags)
    {
        return new LegacyPayBandAgreementEngine(
            new List<ILegacyPayBandAgreementFactor>
            {
                new TieredLegacyPayBandAgreementAdjuster(
                    new List<double> { 6324.0, 6434.0, 7444.0, 9627.0 },
                    new List<double> { 0.81, 0.85, 1.16, 0.82 }),
                new CountryLegacyPayBandAgreementAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.85, ["central"] = 1.16, ["south"] = 0.74 }),
                new MinimumLegacyPayBandAgreementAdjuster(1.9),
                new RoundingLegacyPayBandAgreementAdjuster(0.05),
                new PercentLegacyPayBandAgreementAdjuster(-17.9),
                new FlaggedLegacyPayBandAgreementAdjuster(flags, "legacy-pay-band-agreement-seasonal", -0.0),
            },
            new List<string>(),
            0.2,
            77176.3);
    }

    public double Run(LegacyPayBandAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayBandAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
