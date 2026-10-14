namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive license seat agreement calculation.</summary>
public interface IAdaptiveLicenseSeatAgreementStep
{
    string Describe();
    double Adjust(AdaptiveLicenseSeatAgreement item, double valueValue);
}

public sealed record PercentAdaptiveLicenseSeatAgreementAdjuster(double Percent) : IAdaptiveLicenseSeatAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveLicenseSeatAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveLicenseSeatAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveLicenseSeatAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveLicenseSeatAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptiveLicenseSeatAgreementAdjuster(double Step) : IAdaptiveLicenseSeatAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveLicenseSeatAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveLicenseSeatAgreementAdjuster(double Cap) : IAdaptiveLicenseSeatAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveLicenseSeatAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive license seat agreement steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveLicenseSeatAgreementEngine(List<IAdaptiveLicenseSeatAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveLicenseSeatAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveLicenseSeatAgreementEngine DefaultAdaptiveLicenseSeatAgreementEngine(Registry flags)
    {
        return new AdaptiveLicenseSeatAgreementEngine(
            new List<IAdaptiveLicenseSeatAgreementStep>
            {
                new PercentAdaptiveLicenseSeatAgreementAdjuster(18.3),
                new FlaggedAdaptiveLicenseSeatAgreementAdjuster(flags, "adaptive-license-seat-agreement-manual", 9.6),
                new RoundingAdaptiveLicenseSeatAgreementAdjuster(0.1),
                new CapAdaptiveLicenseSeatAgreementAdjuster(27096.0),
            },
            new List<string>(),
            1.2,
            37414.8);
    }

    public double Compute(AdaptiveLicenseSeatAgreement item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveLicenseSeatAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
