namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip assignment must pass. Check returns its problems, or none.</summary>
public interface IPayslipAssignmentRule
{
    List<string> Check(PayslipAssignment item);
}

public sealed record PayslipAssignmentTitleLengthRule(int MaxLength) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAssignmentRequiredCodeRule(string Message) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipAssignmentRequiredTitleRule(string Message) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipAssignmentOwnerIdLengthRule(int MaxLength) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAssignmentAllowedTitleRule(List<string> Allowed) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAssignmentEmployeeIdLengthRule(int MaxLength) : IPayslipAssignmentRule
{
    public List<string> Check(PayslipAssignment item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip assignment.</summary>
public sealed record PayslipAssignmentValidator(List<IPayslipAssignmentRule> Rules)
{
    public List<string> Validate(PayslipAssignment item)
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

    public void RequireValid(PayslipAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipAssignment> items)
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

/// <summary>Functions of payslip assignment rules.</summary>
public static class PayslipAssignmentRules
{
    public static PayslipAssignmentValidator DefaultPayslipAssignmentValidator()
    {
        return new PayslipAssignmentValidator(
            new List<IPayslipAssignmentRule>
            {
                new PayslipAssignmentTitleLengthRule(16),
                new PayslipAssignmentRequiredCodeRule("payslip assignment needs an entry"),
                new PayslipAssignmentRequiredTitleRule("payslip assignment needs a value"),
                new PayslipAssignmentOwnerIdLengthRule(16),
                new PayslipAssignmentAllowedTitleRule(new List<string> { "adaptive", "pending", "fallback" }),
                new PayslipAssignmentEmployeeIdLengthRule(86400),
            });
    }
}
