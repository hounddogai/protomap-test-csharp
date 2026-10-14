namespace Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>A rule a default payslip mapping must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayslipMappingRule
{
    List<string> Check(DefaultPayslipMapping item);
}

public sealed record DefaultPayslipMappingRequiredReviewerRule(string Message) : IDefaultPayslipMappingRule
{
    public List<string> Check(DefaultPayslipMapping item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipMappingRequiredCodeRule(string Message) : IDefaultPayslipMappingRule
{
    public List<string> Check(DefaultPayslipMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipMappingRevisionRangeRule(int Min, int Max) : IDefaultPayslipMappingRule
{
    public List<string> Check(DefaultPayslipMapping item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default payslip mapping.</summary>
public sealed record DefaultPayslipMappingValidator(List<IDefaultPayslipMappingRule> Rules)
{
    public List<string> Validate(DefaultPayslipMapping item)
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

    public void RequireValid(DefaultPayslipMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayslipMapping> items)
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

/// <summary>Functions of default payslip mapping rules.</summary>
public static class DefaultPayslipMappingRules
{
    public static DefaultPayslipMappingValidator DefaultDefaultPayslipMappingValidator()
    {
        return new DefaultPayslipMappingValidator(
            new List<IDefaultPayslipMappingRule>
            {
                new DefaultPayslipMappingRequiredReviewerRule("default payslip mapping needs this field"),
                new DefaultPayslipMappingRequiredCodeRule("default payslip mapping needs a value"),
                new DefaultPayslipMappingRevisionRangeRule(120, 255),
            });
    }
}
