namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>A rule a regional tax form estimate must pass. Check returns its problems, or none.</summary>
public interface IRegionalTaxFormEstimateRule
{
    List<string> Check(RegionalTaxFormEstimate item);
}

public sealed record RegionalTaxFormEstimateRequiredDepartmentRule(string Message) : IRegionalTaxFormEstimateRule
{
    public List<string> Check(RegionalTaxFormEstimate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTaxFormEstimateRequiredCodeRule(string Message) : IRegionalTaxFormEstimateRule
{
    public List<string> Check(RegionalTaxFormEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTaxFormEstimateAmountRangeRule(double Min, double Max) : IRegionalTaxFormEstimateRule
{
    public List<string> Check(RegionalTaxFormEstimate item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional tax form estimate.</summary>
public sealed record RegionalTaxFormEstimateValidator(List<IRegionalTaxFormEstimateRule> Rules)
{
    public List<string> Validate(RegionalTaxFormEstimate item)
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

    public void RequireValid(RegionalTaxFormEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTaxFormEstimate> items)
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

/// <summary>Functions of regional tax form estimate rules.</summary>
public static class RegionalTaxFormEstimateRules
{
    public static RegionalTaxFormEstimateValidator DefaultRegionalTaxFormEstimateValidator()
    {
        return new RegionalTaxFormEstimateValidator(
            new List<IRegionalTaxFormEstimateRule>
            {
                new RegionalTaxFormEstimateRequiredDepartmentRule("regional tax form estimate needs a value"),
                new RegionalTaxFormEstimateRequiredCodeRule("regional tax form estimate needs this field"),
                new RegionalTaxFormEstimateAmountRangeRule(0.5, 3644.9),
            });
    }
}
