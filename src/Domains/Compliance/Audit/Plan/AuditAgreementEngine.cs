namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit agreement calculation.</summary>
public interface IAuditAgreementModifier
{
    string Describe();
    double Adjust(AuditAgreement item, double valueValue);
}

public sealed record CountryAuditAgreementAdjuster(Dictionary<string, double> Factors) : IAuditAgreementModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AuditAgreementExpiresAtOffsetAdjuster(double Weight) : IAuditAgreementModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedAuditAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAuditAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAuditAgreementAdjuster(double Percent) : IAuditAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAuditAgreementAdjuster(double Step) : IAuditAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAuditAgreementAdjuster(double Minimum) : IAuditAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the audit agreement steps in order and keeps a trace of each.</summary>
public sealed class AuditAgreementEngine(List<IAuditAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditAgreementEngine DefaultAuditAgreementEngine(Registry flags)
    {
        return new AuditAgreementEngine(
            new List<IAuditAgreementModifier>
            {
                new CountryAuditAgreementAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.14, ["central"] = 0.97, ["retail"] = 1.27 }),
                new AuditAgreementExpiresAtOffsetAdjuster(0.34),
                new FlaggedAuditAgreementAdjuster(flags, "audit-agreement-primary", 2.3),
                new PercentAuditAgreementAdjuster(-4.5),
                new RoundingAuditAgreementAdjuster(0.05),
                new MinimumAuditAgreementAdjuster(19.2),
            },
            new List<string>(),
            1.3,
            58885.7);
    }

    public double Price(AuditAgreement item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AuditAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
