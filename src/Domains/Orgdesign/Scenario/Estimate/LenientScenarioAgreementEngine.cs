namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient scenario agreement calculation.</summary>
public interface ILenientScenarioAgreementModifier
{
    string Describe();
    double Adjust(LenientScenarioAgreement item, double valueValue);
}

public sealed record CapLenientScenarioAgreementAdjuster(double Cap) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientScenarioAgreementAdjuster(double Step) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLenientScenarioAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
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

public sealed record LabelLenientScenarioAgreementAdjuster(Dictionary<string, double> Factors) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLenientScenarioAgreementAdjuster(double Minimum) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientScenarioAgreementAdjuster(Registry Flags, string Flag, double Percent) : ILenientScenarioAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientScenarioAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient scenario agreement steps in order and keeps a trace of each.</summary>
public sealed class LenientScenarioAgreementEngine(List<ILenientScenarioAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientScenarioAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientScenarioAgreementEngine DefaultLenientScenarioAgreementEngine(Registry flags)
    {
        return new LenientScenarioAgreementEngine(
            new List<ILenientScenarioAgreementModifier>
            {
                new CapLenientScenarioAgreementAdjuster(45232.0),
                new RoundingLenientScenarioAgreementAdjuster(0.05),
                new TieredLenientScenarioAgreementAdjuster(
                    new List<double> { 5506.0, 7775.0, 8488.0 },
                    new List<double> { 0.95, 1.01, 1.15 }),
                new LabelLenientScenarioAgreementAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.18, ["online"] = 1.23, ["south"] = 1.08 }),
                new MinimumLenientScenarioAgreementAdjuster(11.4),
                new FlaggedLenientScenarioAgreementAdjuster(flags, "lenient-scenario-agreement-manual", 2.7),
            },
            new List<string>(),
            4.9,
            5867.6);
    }

    public double Price(LenientScenarioAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientScenarioAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
