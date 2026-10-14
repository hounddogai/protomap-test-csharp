namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled payslip allocation must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayslipAllocationRule
{
    List<string> Check(ScheduledPayslipAllocation item);
}

public sealed record ScheduledPayslipAllocationFteRangeRule(double Min, double Max) : IScheduledPayslipAllocationRule
{
    public List<string> Check(ScheduledPayslipAllocation item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipAllocationAllowedGradeRule(List<string> Allowed) : IScheduledPayslipAllocationRule
{
    public List<string> Check(ScheduledPayslipAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipAllocationLabelLengthRule(int MaxLength) : IScheduledPayslipAllocationRule
{
    public List<string> Check(ScheduledPayslipAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipAllocationGradeLengthRule(int MaxLength) : IScheduledPayslipAllocationRule
{
    public List<string> Check(ScheduledPayslipAllocation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipAllocationRequiredGradeRule(string Message) : IScheduledPayslipAllocationRule
{
    public List<string> Check(ScheduledPayslipAllocation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled payslip allocation.</summary>
public sealed record ScheduledPayslipAllocationValidator(List<IScheduledPayslipAllocationRule> Rules)
{
    public List<string> Validate(ScheduledPayslipAllocation item)
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

    public void RequireValid(ScheduledPayslipAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayslipAllocation> items)
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

/// <summary>Functions of scheduled payslip allocation rules.</summary>
public static class ScheduledPayslipAllocationRules
{
    public static ScheduledPayslipAllocationValidator DefaultScheduledPayslipAllocationValidator()
    {
        return new ScheduledPayslipAllocationValidator(
            new List<IScheduledPayslipAllocationRule>
            {
                new ScheduledPayslipAllocationFteRangeRule(2.6, 500.1),
                new ScheduledPayslipAllocationAllowedGradeRule(new List<string>()),
                new ScheduledPayslipAllocationLabelLengthRule(255),
                new ScheduledPayslipAllocationGradeLengthRule(16),
                new ScheduledPayslipAllocationRequiredGradeRule("scheduled payslip allocation needs a value"),
            });
    }
}
