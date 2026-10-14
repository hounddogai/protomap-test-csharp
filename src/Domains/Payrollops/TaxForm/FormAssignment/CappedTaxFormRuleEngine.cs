namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>One step of the capped tax form rule calculation.</summary>
public interface ICappedTaxFormRuleModifier
{
    string Describe();
    double Adjust(CappedTaxFormRule item, double valueValue);
}

public sealed record MinimumCappedTaxFormRuleAdjuster(double Minimum) : ICappedTaxFormRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedTaxFormRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedTaxFormRuleLeaveDaysOffsetAdjuster(double Weight) : ICappedTaxFormRuleModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(CappedTaxFormRule item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentCappedTaxFormRuleAdjuster(double Percent) : ICappedTaxFormRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedTaxFormRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped tax form rule steps in order and keeps a trace of each.</summary>
public sealed class CappedTaxFormRuleEngine(List<ICappedTaxFormRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedTaxFormRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedTaxFormRuleEngine DefaultCappedTaxFormRuleEngine()
    {
        return new CappedTaxFormRuleEngine(
            new List<ICappedTaxFormRuleModifier>
            {
                new MinimumCappedTaxFormRuleAdjuster(47.3),
                new CappedTaxFormRuleLeaveDaysOffsetAdjuster(0.68),
                new PercentCappedTaxFormRuleAdjuster(10.4),
            },
            new List<string>(),
            2.7,
            50555.3);
    }

    public double Evaluate(CappedTaxFormRule item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedTaxFormRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
