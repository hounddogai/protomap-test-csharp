namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan agreement calculation.</summary>
public interface ISuccessionPlanAgreementStep
{
    string Describe();
    double Adjust(SuccessionPlanAgreement item, double valueValue);
}

public sealed record TieredSuccessionPlanAgreementAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanAgreement item, double valueValue)
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

public sealed record CountrySuccessionPlanAgreementAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanAgreementStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(SuccessionPlanAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSuccessionPlanAgreementAdjuster(double Minimum) : ISuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSuccessionPlanAgreementAdjuster(double Cap) : ISuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSuccessionPlanAgreementAdjuster(double Step) : ISuccessionPlanAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the succession plan agreement steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanAgreementEngine(List<ISuccessionPlanAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanAgreementEngine DefaultSuccessionPlanAgreementEngine()
    {
        return new SuccessionPlanAgreementEngine(
            new List<ISuccessionPlanAgreementStep>
            {
                new TieredSuccessionPlanAgreementAdjuster(
                    new List<double> { 5682.0, 6598.0, 8381.0, 9026.0 },
                    new List<double> { 0.95, 1.16, 1.02, 0.95 }),
                new CountrySuccessionPlanAgreementAdjuster(new Dictionary<string, double> { ["west"] = 1.07 }),
                new MinimumSuccessionPlanAgreementAdjuster(43.5),
                new CapSuccessionPlanAgreementAdjuster(30128.0),
                new RoundingSuccessionPlanAgreementAdjuster(0.25),
            },
            new List<string>(),
            2.7,
            7782.5);
    }

    public double Run(SuccessionPlanAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
