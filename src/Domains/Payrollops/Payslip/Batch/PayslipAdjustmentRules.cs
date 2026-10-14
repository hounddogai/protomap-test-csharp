namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip adjustment must pass. Check returns its problems, or none.</summary>
public interface IPayslipAdjustmentRule
{
    List<string> Check(PayslipAdjustment item);
}

public sealed record PayslipAdjustmentAllowedNameRule(List<string> Allowed) : IPayslipAdjustmentRule
{
    public List<string> Check(PayslipAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAdjustmentOwnerIdLengthRule(int MaxLength) : IPayslipAdjustmentRule
{
    public List<string> Check(PayslipAdjustment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipAdjustmentCodeLengthRule(int MaxLength) : IPayslipAdjustmentRule
{
    public List<string> Check(PayslipAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip adjustment.</summary>
public sealed record PayslipAdjustmentValidator(List<IPayslipAdjustmentRule> Rules)
{
    public List<string> Validate(PayslipAdjustment item)
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

    public void RequireValid(PayslipAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipAdjustment> items)
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

/// <summary>Functions of payslip adjustment rules.</summary>
public static class PayslipAdjustmentRules
{
    public static PayslipAdjustmentValidator DefaultPayslipAdjustmentValidator()
    {
        return new PayslipAdjustmentValidator(
            new List<IPayslipAdjustmentRule>
            {
                new PayslipAdjustmentAllowedNameRule(new List<string> { "fallback", "batched" }),
                new PayslipAdjustmentOwnerIdLengthRule(16),
                new PayslipAdjustmentCodeLengthRule(80),
            });
    }
}
