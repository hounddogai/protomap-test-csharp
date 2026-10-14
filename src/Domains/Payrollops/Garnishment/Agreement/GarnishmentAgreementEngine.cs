namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment agreement calculation.</summary>
public interface IGarnishmentAgreementModifier
{
    string Describe();
    double Adjust(GarnishmentAgreement item, double valueValue);
}

public sealed record PercentGarnishmentAgreementAdjuster(double Percent) : IGarnishmentAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GarnishmentAgreementFloorValueOffsetAdjuster(double Weight) : IGarnishmentAgreementModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(GarnishmentAgreement item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumGarnishmentAgreementAdjuster(double Minimum) : IGarnishmentAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGarnishmentAgreementAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment agreement steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentAgreementEngine(List<IGarnishmentAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentAgreementEngine DefaultGarnishmentAgreementEngine(Registry flags)
    {
        return new GarnishmentAgreementEngine(
            new List<IGarnishmentAgreementModifier>
            {
                new PercentGarnishmentAgreementAdjuster(-14.7),
                new GarnishmentAgreementFloorValueOffsetAdjuster(0.85),
                new MinimumGarnishmentAgreementAdjuster(16.1),
                new FlaggedGarnishmentAgreementAdjuster(flags, "garnishment-agreement-manual", 6.7),
            },
            new List<string>(),
            2.5,
            5720.8);
    }

    public double Price(GarnishmentAgreement item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
