namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual agreement calculation.</summary>
public interface IAccrualAgreementFactor
{
    string Describe();
    double Adjust(AccrualAgreement item, double valueValue);
}

public sealed record TieredAccrualAgreementAdjuster(List<double> Bounds, List<double> Rates) : IAccrualAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualAgreement item, double valueValue)
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

public sealed record FlaggedAccrualAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAccrualAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccrualAgreementAdjuster(double Cap) : IAccrualAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAccrualAgreementAdjuster(double Minimum) : IAccrualAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAccrualAgreementAdjuster(double Step) : IAccrualAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual agreement steps in order and keeps a trace of each.</summary>
public sealed class AccrualAgreementEngine(List<IAccrualAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualAgreementEngine DefaultAccrualAgreementEngine(Registry flags)
    {
        return new AccrualAgreementEngine(
            new List<IAccrualAgreementFactor>
            {
                new TieredAccrualAgreementAdjuster(
                    new List<double> { 5698.0, 5782.0, 9138.0 },
                    new List<double> { 1.2, 0.94, 0.97 }),
                new FlaggedAccrualAgreementAdjuster(flags, "accrual-agreement-seasonal", 8.7),
                new CapAccrualAgreementAdjuster(1488.0),
                new MinimumAccrualAgreementAdjuster(22.6),
                new RoundingAccrualAgreementAdjuster(0.5),
            },
            new List<string>(),
            0.4,
            44401.3);
    }

    public double Run(AccrualAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
