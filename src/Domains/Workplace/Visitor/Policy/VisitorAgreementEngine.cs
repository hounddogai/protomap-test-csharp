namespace Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>One step of the visitor agreement calculation.</summary>
public interface IVisitorAgreementFactor
{
    string Describe();
    double Adjust(VisitorAgreement item, double valueValue);
}

public sealed record PercentVisitorAgreementAdjuster(double Percent) : IVisitorAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVisitorAgreementAdjuster(List<double> Bounds, List<double> Rates) : IVisitorAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorAgreement item, double valueValue)
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

public sealed record CapVisitorAgreementAdjuster(double Cap) : IVisitorAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumVisitorAgreementAdjuster(double Minimum) : IVisitorAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the visitor agreement steps in order and keeps a trace of each.</summary>
public sealed class VisitorAgreementEngine(List<IVisitorAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorAgreementEngine DefaultVisitorAgreementEngine()
    {
        return new VisitorAgreementEngine(
            new List<IVisitorAgreementFactor>
            {
                new PercentVisitorAgreementAdjuster(9.5),
                new TieredVisitorAgreementAdjuster(
                    new List<double> { 409.0, 7416.0 },
                    new List<double> { 1.1, 1.02 }),
                new CapVisitorAgreementAdjuster(18503.0),
                new MinimumVisitorAgreementAdjuster(43.0),
            },
            new List<string>(),
            2.0,
            6740.7);
    }

    public double Price(VisitorAgreement item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
