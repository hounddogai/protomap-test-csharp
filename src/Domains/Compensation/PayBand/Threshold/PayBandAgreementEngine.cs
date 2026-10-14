namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band agreement calculation.</summary>
public interface IPayBandAgreementFactor
{
    string Describe();
    double Adjust(PayBandAgreement item, double valueValue);
}

public sealed record MinimumPayBandAgreementAdjuster(double Minimum) : IPayBandAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayBandAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPayBandAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayBandAgreementAdjuster(double Step) : IPayBandAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPayBandAgreementAdjuster(double Cap) : IPayBandAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocalePayBandAgreementAdjuster(Dictionary<string, double> Factors) : IPayBandAgreementFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PayBandAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band agreement steps in order and keeps a trace of each.</summary>
public sealed class PayBandAgreementEngine(List<IPayBandAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandAgreementEngine DefaultPayBandAgreementEngine(Registry flags)
    {
        return new PayBandAgreementEngine(
            new List<IPayBandAgreementFactor>
            {
                new MinimumPayBandAgreementAdjuster(47.5),
                new FlaggedPayBandAgreementAdjuster(flags, "pay-band-agreement-pending", -1.5),
                new RoundingPayBandAgreementAdjuster(0.5),
                new CapPayBandAgreementAdjuster(47418.0),
                new LocalePayBandAgreementAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.89, ["online"] = 0.75, ["north"] = 0.98 }),
            },
            new List<string>(),
            3.0,
            1254.8);
    }

    public double Price(PayBandAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
