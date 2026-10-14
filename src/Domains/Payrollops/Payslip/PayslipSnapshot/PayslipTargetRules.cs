namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>A rule a payslip target must pass. Check returns its problems, or none.</summary>
public interface IPayslipTargetRule
{
    List<string> Check(PayslipTarget item);
}

public sealed record PayslipTargetCostRangeRule(double Min, double Max) : IPayslipTargetRule
{
    public List<string> Check(PayslipTarget item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTargetSourceLengthRule(int MaxLength) : IPayslipTargetRule
{
    public List<string> Check(PayslipTarget item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTargetLimitRangeRule(int Min, int Max) : IPayslipTargetRule
{
    public List<string> Check(PayslipTarget item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTargetRequiredSourceRule(string Message) : IPayslipTargetRule
{
    public List<string> Check(PayslipTarget item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip target.</summary>
public sealed record PayslipTargetValidator(List<IPayslipTargetRule> Rules)
{
    public List<string> Validate(PayslipTarget item)
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

    public void RequireValid(PayslipTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipTarget> items)
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

/// <summary>Functions of payslip target rules.</summary>
public static class PayslipTargetRules
{
    public static PayslipTargetValidator DefaultPayslipTargetValidator()
    {
        return new PayslipTargetValidator(
            new List<IPayslipTargetRule>
            {
                new PayslipTargetCostRangeRule(0.7, 1731.2),
                new PayslipTargetSourceLengthRule(32),
                new PayslipTargetLimitRangeRule(80, 86400),
                new PayslipTargetRequiredSourceRule("payslip target needs a value"),
            });
    }
}
