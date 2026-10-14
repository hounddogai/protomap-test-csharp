namespace Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>A rule a manual payslip allocation must pass. Check returns its problems, or none.</summary>
public interface IManualPayslipAllocationRule
{
    List<string> Check(ManualPayslipAllocation item);
}

public sealed record ManualPayslipAllocationRevisionRangeRule(int Min, int Max) : IManualPayslipAllocationRule
{
    public List<string> Check(ManualPayslipAllocation item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPayslipAllocationRequiredPhoneRule(string Message) : IManualPayslipAllocationRule
{
    public List<string> Check(ManualPayslipAllocation item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPayslipAllocationCountryLengthRule(int MaxLength) : IManualPayslipAllocationRule
{
    public List<string> Check(ManualPayslipAllocation item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual payslip allocation.</summary>
public sealed record ManualPayslipAllocationValidator(List<IManualPayslipAllocationRule> Rules)
{
    public List<string> Validate(ManualPayslipAllocation item)
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

    public void RequireValid(ManualPayslipAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPayslipAllocation> items)
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

/// <summary>Functions of manual payslip allocation rules.</summary>
public static class ManualPayslipAllocationRules
{
    public static ManualPayslipAllocationValidator DefaultManualPayslipAllocationValidator()
    {
        return new ManualPayslipAllocationValidator(
            new List<IManualPayslipAllocationRule>
            {
                new ManualPayslipAllocationRevisionRangeRule(80, 120),
                new ManualPayslipAllocationRequiredPhoneRule("manual payslip allocation needs an entry"),
                new ManualPayslipAllocationCountryLengthRule(80),
            });
    }
}
