namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction agreement calculation.</summary>
public interface IDeductionAgreementModifier
{
    string Describe();
    double Adjust(DeductionAgreement item, double valueValue);
}

public sealed record MinimumDeductionAgreementAdjuster(double Minimum) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDeductionAgreementAdjuster(double Percent) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeductionAgreementCeilingOffsetAdjuster(double Weight) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record RoundingDeductionAgreementAdjuster(double Step) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDeductionAgreementAdjuster(List<double> Bounds, List<double> Rates) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
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

public sealed record SourceDeductionAgreementAdjuster(Dictionary<string, double> Factors) : IDeductionAgreementModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DeductionAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the deduction agreement steps in order and keeps a trace of each.</summary>
public sealed class DeductionAgreementEngine(List<IDeductionAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionAgreementEngine DefaultDeductionAgreementEngine()
    {
        return new DeductionAgreementEngine(
            new List<IDeductionAgreementModifier>
            {
                new MinimumDeductionAgreementAdjuster(22.5),
                new PercentDeductionAgreementAdjuster(12.8),
                new DeductionAgreementCeilingOffsetAdjuster(0.69),
                new RoundingDeductionAgreementAdjuster(0.25),
                new TieredDeductionAgreementAdjuster(
                    new List<double> { 5573.0, 9564.0 },
                    new List<double> { 0.87, 1.09 }),
                new SourceDeductionAgreementAdjuster(new Dictionary<string, double> { ["east"] = 1.24 }),
            },
            new List<string>(),
            2.5,
            74932.6);
    }

    public double Compute(DeductionAgreement item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
