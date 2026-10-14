namespace Acme.Workforce.Payrollops.Payslip.Notice;

/// <summary>A rule a payslip allocation must pass. Check returns its problems, or none.</summary>
public interface IPayslipAllocationRule
{
    List<string> Check(PayslipAllocation item);
}

public sealed record PayslipAllocationBadgeLengthRule(int MaxLength) : IPayslipAllocationRule
{
    public List<string> Check(PayslipAllocation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAllocationRequiredTitleRule(string Message) : IPayslipAllocationRule
{
    public List<string> Check(PayslipAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipAllocationCodeLengthRule(int MaxLength) : IPayslipAllocationRule
{
    public List<string> Check(PayslipAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip allocation.</summary>
public sealed record PayslipAllocationValidator(List<IPayslipAllocationRule> Rules)
{
    public List<string> Validate(PayslipAllocation item)
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

    public void RequireValid(PayslipAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipAllocation> items)
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

/// <summary>Functions of payslip allocation rules.</summary>
public static class PayslipAllocationRules
{
    public static PayslipAllocationValidator DefaultPayslipAllocationValidator()
    {
        return new PayslipAllocationValidator(
            new List<IPayslipAllocationRule>
            {
                new PayslipAllocationBadgeLengthRule(3600),
                new PayslipAllocationRequiredTitleRule("payslip allocation needs an entry"),
                new PayslipAllocationCodeLengthRule(255),
            });
    }
}
