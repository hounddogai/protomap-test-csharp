namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip rule must pass. Check returns its problems, or none.</summary>
public interface IPayslipRuleRule
{
    List<string> Check(PayslipRule item);
}

public sealed record PayslipRuleCodeLengthRule(int MaxLength) : IPayslipRuleRule
{
    public List<string> Check(PayslipRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRuleRequiredCodeRule(string Message) : IPayslipRuleRule
{
    public List<string> Check(PayslipRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipRuleAllowedCodeRule(List<string> Allowed) : IPayslipRuleRule
{
    public List<string> Check(PayslipRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRuleAmountRangeRule(double Min, double Max) : IPayslipRuleRule
{
    public List<string> Check(PayslipRule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipRuleCapacityRangeRule(int Min, int Max) : IPayslipRuleRule
{
    public List<string> Check(PayslipRule item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a payslip rule.</summary>
public sealed record PayslipRuleValidator(List<IPayslipRuleRule> Rules)
{
    public List<string> Validate(PayslipRule item)
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

    public void RequireValid(PayslipRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipRule> items)
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

/// <summary>Functions of payslip rule rules.</summary>
public static class PayslipRuleRules
{
    public static PayslipRuleValidator DefaultPayslipRuleValidator()
    {
        return new PayslipRuleValidator(
            new List<IPayslipRuleRule>
            {
                new PayslipRuleCodeLengthRule(64),
                new PayslipRuleRequiredCodeRule("payslip rule needs this field"),
                new PayslipRuleAllowedCodeRule(new List<string> { "pending" }),
                new PayslipRuleAmountRangeRule(9.4, 4264.2),
                new PayslipRuleCapacityRangeRule(80, 255),
            });
    }
}
