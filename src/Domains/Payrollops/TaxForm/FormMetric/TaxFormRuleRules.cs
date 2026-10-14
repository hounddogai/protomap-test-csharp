namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>A rule a tax form rule must pass. Check returns its problems, or none.</summary>
public interface ITaxFormRuleRule
{
    List<string> Check(TaxFormRule item);
}

public sealed record TaxFormRuleMarginRangeRule(double Min, double Max) : ITaxFormRuleRule
{
    public List<string> Check(TaxFormRule item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormRuleEndsAtRangeRule(int Min, int Max) : ITaxFormRuleRule
{
    public List<string> Check(TaxFormRule item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRuleCodePrefixRule(string Prefix) : ITaxFormRuleRule
{
    public List<string> Check(TaxFormRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRuleLeaveDaysRangeRule(int Min, int Max) : ITaxFormRuleRule
{
    public List<string> Check(TaxFormRule item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormRuleCodeLengthRule(int MaxLength) : ITaxFormRuleRule
{
    public List<string> Check(TaxFormRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form rule.</summary>
public sealed record TaxFormRuleValidator(List<ITaxFormRuleRule> Rules)
{
    public List<string> Validate(TaxFormRule item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(TaxFormRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormRule> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of tax form rule rules.</summary>
public static class TaxFormRuleRules
{
    public static TaxFormRuleValidator DefaultTaxFormRuleValidator()
    {
        return new TaxFormRuleValidator(
            new List<ITaxFormRuleRule>
            {
                new TaxFormRuleMarginRangeRule(5.8, 4177.1),
                new TaxFormRuleEndsAtRangeRule(255, 1000),
                new TaxFormRuleCodePrefixRule("Q"),
                new TaxFormRuleLeaveDaysRangeRule(80, 80),
                new TaxFormRuleCodeLengthRule(64),
            });
    }
}
