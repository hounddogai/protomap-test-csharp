namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled payslip template must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayslipTemplateRule
{
    List<string> Check(ScheduledPayslipTemplate item);
}

public sealed record ScheduledPayslipTemplateLeaveDaysRangeRule(int Min, int Max) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPayslipTemplateRequiredCodeRule(string Message) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipTemplateRequiredLabelRule(string Message) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipTemplateCodeLengthRule(int MaxLength) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipTemplateAllowedNameRule(List<string> Allowed) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipTemplateCodePrefixRule(string Prefix) : IScheduledPayslipTemplateRule
{
    public List<string> Check(ScheduledPayslipTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled payslip template.</summary>
public sealed record ScheduledPayslipTemplateValidator(List<IScheduledPayslipTemplateRule> Rules)
{
    public List<string> Validate(ScheduledPayslipTemplate item)
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

    public void RequireValid(ScheduledPayslipTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayslipTemplate> items)
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

/// <summary>Functions of scheduled payslip template rules.</summary>
public static class ScheduledPayslipTemplateRules
{
    public static ScheduledPayslipTemplateValidator DefaultScheduledPayslipTemplateValidator()
    {
        return new ScheduledPayslipTemplateValidator(
            new List<IScheduledPayslipTemplateRule>
            {
                new ScheduledPayslipTemplateLeaveDaysRangeRule(64, 3600),
                new ScheduledPayslipTemplateRequiredCodeRule("scheduled payslip template needs an entry"),
                new ScheduledPayslipTemplateRequiredLabelRule("scheduled payslip template needs a value"),
                new ScheduledPayslipTemplateCodeLengthRule(80),
                new ScheduledPayslipTemplateAllowedNameRule(new List<string>()),
                new ScheduledPayslipTemplateCodePrefixRule("REF"),
            });
    }
}
