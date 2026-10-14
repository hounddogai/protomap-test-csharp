namespace Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>A rule a seasonal payslip baseline must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayslipBaselineRule
{
    List<string> Check(SeasonalPayslipBaseline item);
}

public sealed record SeasonalPayslipBaselineCodePrefixRule(string Prefix) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipBaselineCodeLengthRule(int MaxLength) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipBaselineRequiredDepartmentRule(string Message) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipBaselineRequiredCurrencyRule(string Message) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipBaselineWeightRangeRule(double Min, double Max) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayslipBaselineRequiredCodeRule(string Message) : ISeasonalPayslipBaselineRule
{
    public List<string> Check(SeasonalPayslipBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal payslip baseline.</summary>
public sealed record SeasonalPayslipBaselineValidator(List<ISeasonalPayslipBaselineRule> Rules)
{
    public List<string> Validate(SeasonalPayslipBaseline item)
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

    public void RequireValid(SeasonalPayslipBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayslipBaseline> items)
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

/// <summary>Functions of seasonal payslip baseline rules.</summary>
public static class SeasonalPayslipBaselineRules
{
    public static SeasonalPayslipBaselineValidator DefaultSeasonalPayslipBaselineValidator()
    {
        return new SeasonalPayslipBaselineValidator(
            new List<ISeasonalPayslipBaselineRule>
            {
                new SeasonalPayslipBaselineCodePrefixRule("ACM"),
                new SeasonalPayslipBaselineCodeLengthRule(120),
                new SeasonalPayslipBaselineRequiredDepartmentRule("seasonal payslip baseline needs a value"),
                new SeasonalPayslipBaselineRequiredCurrencyRule("seasonal payslip baseline needs this field"),
                new SeasonalPayslipBaselineWeightRangeRule(6.5, 3855.8),
                new SeasonalPayslipBaselineRequiredCodeRule("seasonal payslip baseline needs this field"),
            });
    }
}
