namespace Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>A rule a tax form group must pass. Check returns its problems, or none.</summary>
public interface ITaxFormGroupRule
{
    List<string> Check(TaxFormGroup item);
}

public sealed record TaxFormGroupMinimumRangeRule(double Min, double Max) : ITaxFormGroupRule
{
    public List<string> Check(TaxFormGroup item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormGroupCodePrefixRule(string Prefix) : ITaxFormGroupRule
{
    public List<string> Check(TaxFormGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormGroupRequiredLabelRule(string Message) : ITaxFormGroupRule
{
    public List<string> Check(TaxFormGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form group.</summary>
public sealed record TaxFormGroupValidator(List<ITaxFormGroupRule> Rules)
{
    public List<string> Validate(TaxFormGroup item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(TaxFormGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormGroup> items)
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

/// <summary>Functions of tax form group rules.</summary>
public static class TaxFormGroupRules
{
    public static TaxFormGroupValidator DefaultTaxFormGroupValidator()
    {
        return new TaxFormGroupValidator(
            new List<ITaxFormGroupRule>
            {
                new TaxFormGroupMinimumRangeRule(8.2, 2376.7),
                new TaxFormGroupCodePrefixRule("Q"),
                new TaxFormGroupRequiredLabelRule("tax form group needs a value"),
            });
    }
}
