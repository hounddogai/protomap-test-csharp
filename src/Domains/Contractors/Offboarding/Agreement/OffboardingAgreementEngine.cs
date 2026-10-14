namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding agreement calculation.</summary>
public interface IOffboardingAgreementFactor
{
    string Describe();
    double Adjust(OffboardingAgreement item, double valueValue);
}

public sealed record TieredOffboardingAgreementAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
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

public sealed record LabelOffboardingAgreementAdjuster(Dictionary<string, double> Factors) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedOffboardingAgreementAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOffboardingAgreementAdjuster(double Cap) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record OffboardingAgreementLeaveDaysOffsetAdjuster(double Weight) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record RoundingOffboardingAgreementAdjuster(double Step) : IOffboardingAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the offboarding agreement steps in order and keeps a trace of each.</summary>
public sealed class OffboardingAgreementEngine(List<IOffboardingAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingAgreementEngine DefaultOffboardingAgreementEngine(Registry flags)
    {
        return new OffboardingAgreementEngine(
            new List<IOffboardingAgreementFactor>
            {
                new TieredOffboardingAgreementAdjuster(
                    new List<double> { 509.0, 4508.0, 5578.0, 9782.0 },
                    new List<double> { 1.01, 0.89, 1.2, 1.02 }),
                new LabelOffboardingAgreementAdjuster(new Dictionary<string, double> { ["central"] = 0.94 }),
                new FlaggedOffboardingAgreementAdjuster(flags, "offboarding-agreement-pending", 7.9),
                new CapOffboardingAgreementAdjuster(2608.0),
                new OffboardingAgreementLeaveDaysOffsetAdjuster(1.99),
                new RoundingOffboardingAgreementAdjuster(0.5),
            },
            new List<string>(),
            1.7,
            5492.7);
    }

    public double Evaluate(OffboardingAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
