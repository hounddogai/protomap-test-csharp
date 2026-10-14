namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat agreement calculation.</summary>
public interface ILicenseSeatAgreementModifier
{
    string Describe();
    double Adjust(LicenseSeatAgreement item, double valueValue);
}

public sealed record LicenseSeatAgreementMaximumOffsetAdjuster(double Weight) : ILicenseSeatAgreementModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LicenseSeatAgreement item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record RoundingLicenseSeatAgreementAdjuster(double Step) : ILicenseSeatAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLicenseSeatAgreementAdjuster(double Cap) : ILicenseSeatAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat agreement steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatAgreementEngine(List<ILicenseSeatAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatAgreementEngine DefaultLicenseSeatAgreementEngine()
    {
        return new LicenseSeatAgreementEngine(
            new List<ILicenseSeatAgreementModifier>
            {
                new LicenseSeatAgreementMaximumOffsetAdjuster(0.64),
                new RoundingLicenseSeatAgreementAdjuster(0.25),
                new CapLicenseSeatAgreementAdjuster(16890.0),
            },
            new List<string>(),
            1.2,
            90886.5);
    }

    public double Compute(LicenseSeatAgreement item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
