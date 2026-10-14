namespace Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>One step of the career path agreement calculation.</summary>
public interface ICareerPathAgreementModifier
{
    string Describe();
    double Adjust(CareerPathAgreement item, double valueValue);
}

public sealed record TieredCareerPathAgreementAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathAgreement item, double valueValue)
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

public sealed record PercentCareerPathAgreementAdjuster(double Percent) : ICareerPathAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionCareerPathAgreementAdjuster(Dictionary<string, double> Factors) : ICareerPathAgreementModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CareerPathAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path agreement steps in order and keeps a trace of each.</summary>
public sealed class CareerPathAgreementEngine(List<ICareerPathAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathAgreementEngine DefaultCareerPathAgreementEngine()
    {
        return new CareerPathAgreementEngine(
            new List<ICareerPathAgreementModifier>
            {
                new TieredCareerPathAgreementAdjuster(
                    new List<double> { 6810.0, 8186.0, 9744.0 },
                    new List<double> { 1.04, 1.12, 1.07 }),
                new PercentCareerPathAgreementAdjuster(20.8),
                new RegionCareerPathAgreementAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.36, ["east"] = 0.77 }),
            },
            new List<string>(),
            1.8,
            88224.1);
    }

    public double Price(CareerPathAgreement item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
