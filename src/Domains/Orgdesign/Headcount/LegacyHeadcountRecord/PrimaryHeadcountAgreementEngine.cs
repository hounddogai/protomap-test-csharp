namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary headcount agreement calculation.</summary>
public interface IPrimaryHeadcountAgreementModifier
{
    string Describe();
    double Adjust(PrimaryHeadcountAgreement item, double valueValue);
}

public sealed record PrimaryHeadcountAgreementCreatedAtOffsetAdjuster(double Weight) : IPrimaryHeadcountAgreementModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PrimaryHeadcountAgreement item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapPrimaryHeadcountAgreementAdjuster(double Cap) : IPrimaryHeadcountAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryHeadcountAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryHeadcountAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryHeadcountAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryHeadcountAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryHeadcountAgreementAdjuster(double Step) : IPrimaryHeadcountAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryHeadcountAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPrimaryHeadcountAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryHeadcountAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryHeadcountAgreement item, double valueValue)
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

/// <summary>Runs the primary headcount agreement steps in order and keeps a trace of each.</summary>
public sealed class PrimaryHeadcountAgreementEngine(List<IPrimaryHeadcountAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryHeadcountAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryHeadcountAgreementEngine DefaultPrimaryHeadcountAgreementEngine(Registry flags)
    {
        return new PrimaryHeadcountAgreementEngine(
            new List<IPrimaryHeadcountAgreementModifier>
            {
                new PrimaryHeadcountAgreementCreatedAtOffsetAdjuster(1.73),
                new CapPrimaryHeadcountAgreementAdjuster(26019.0),
                new FlaggedPrimaryHeadcountAgreementAdjuster(flags, "primary-headcount-agreement-fallback", 7.5),
                new RoundingPrimaryHeadcountAgreementAdjuster(5.0),
                new TieredPrimaryHeadcountAgreementAdjuster(
                    new List<double> { 4005.0, 7106.0 },
                    new List<double> { 1.06, 1.05 }),
            },
            new List<string>(),
            0.6,
            83375.2);
    }

    public double Compute(PrimaryHeadcountAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryHeadcountAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
