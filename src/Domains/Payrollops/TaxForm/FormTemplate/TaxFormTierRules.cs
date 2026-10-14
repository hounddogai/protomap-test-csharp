namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>A rule a tax form tier must pass. Check returns its problems, or none.</summary>
public interface ITaxFormTierRule
{
    List<string> Check(TaxFormTier item);
}

public sealed record TaxFormTierRequiredCodeRule(string Message) : ITaxFormTierRule
{
    public List<string> Check(TaxFormTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormTierContactEmailLengthRule(int MaxLength) : ITaxFormTierRule
{
    public List<string> Check(TaxFormTier item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormTierRequiredNameRule(string Message) : ITaxFormTierRule
{
    public List<string> Check(TaxFormTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form tier.</summary>
public sealed record TaxFormTierValidator(List<ITaxFormTierRule> Rules)
{
    public List<string> Validate(TaxFormTier item)
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

    public void RequireValid(TaxFormTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormTier> items)
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

/// <summary>Functions of tax form tier rules.</summary>
public static class TaxFormTierRules
{
    public static TaxFormTierValidator DefaultTaxFormTierValidator()
    {
        return new TaxFormTierValidator(
            new List<ITaxFormTierRule>
            {
                new TaxFormTierRequiredCodeRule("tax form tier needs a value"),
                new TaxFormTierContactEmailLengthRule(120),
                new TaxFormTierRequiredNameRule("tax form tier needs this field"),
            });
    }
}
