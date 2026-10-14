namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>A rule a regional deduction template must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeductionTemplateRule
{
    List<string> Check(RegionalDeductionTemplate item);
}

public sealed record RegionalDeductionTemplateSummaryLengthRule(int MaxLength) : IRegionalDeductionTemplateRule
{
    public List<string> Check(RegionalDeductionTemplate item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionTemplateRequiredNameRule(string Message) : IRegionalDeductionTemplateRule
{
    public List<string> Check(RegionalDeductionTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionTemplateRequiredExternalRefRule(string Message) : IRegionalDeductionTemplateRule
{
    public List<string> Check(RegionalDeductionTemplate item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional deduction template.</summary>
public sealed record RegionalDeductionTemplateValidator(List<IRegionalDeductionTemplateRule> Rules)
{
    public List<string> Validate(RegionalDeductionTemplate item)
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

    public void RequireValid(RegionalDeductionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeductionTemplate> items)
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

/// <summary>Functions of regional deduction template rules.</summary>
public static class RegionalDeductionTemplateRules
{
    public static RegionalDeductionTemplateValidator DefaultRegionalDeductionTemplateValidator()
    {
        return new RegionalDeductionTemplateValidator(
            new List<IRegionalDeductionTemplateRule>
            {
                new RegionalDeductionTemplateSummaryLengthRule(3600),
                new RegionalDeductionTemplateRequiredNameRule("regional deduction template needs an entry"),
                new RegionalDeductionTemplateRequiredExternalRefRule("regional deduction template needs an entry"),
            });
    }
}
