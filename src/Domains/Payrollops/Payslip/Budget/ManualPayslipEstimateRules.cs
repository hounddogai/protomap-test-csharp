namespace Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>A rule a manual payslip estimate must pass. Check returns its problems, or none.</summary>
public interface IManualPayslipEstimateRule
{
    List<string> Check(ManualPayslipEstimate item);
}

public sealed record ManualPayslipEstimateRequiredCodeRule(string Message) : IManualPayslipEstimateRule
{
    public List<string> Check(ManualPayslipEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPayslipEstimateCodeLengthRule(int MaxLength) : IManualPayslipEstimateRule
{
    public List<string> Check(ManualPayslipEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPayslipEstimateRequiredDepartmentRule(string Message) : IManualPayslipEstimateRule
{
    public List<string> Check(ManualPayslipEstimate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual payslip estimate.</summary>
public sealed record ManualPayslipEstimateValidator(List<IManualPayslipEstimateRule> Rules)
{
    public List<string> Validate(ManualPayslipEstimate item)
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

    public void RequireValid(ManualPayslipEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPayslipEstimate> items)
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

/// <summary>Functions of manual payslip estimate rules.</summary>
public static class ManualPayslipEstimateRules
{
    public static ManualPayslipEstimateValidator DefaultManualPayslipEstimateValidator()
    {
        return new ManualPayslipEstimateValidator(
            new List<IManualPayslipEstimateRule>
            {
                new ManualPayslipEstimateRequiredCodeRule("manual payslip estimate needs a value"),
                new ManualPayslipEstimateCodeLengthRule(64),
                new ManualPayslipEstimateRequiredDepartmentRule("manual payslip estimate needs this field"),
            });
    }
}
