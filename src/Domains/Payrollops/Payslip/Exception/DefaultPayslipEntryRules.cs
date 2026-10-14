namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default payslip entry must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayslipEntryRule
{
    List<string> Check(DefaultPayslipEntry item);
}

public sealed record DefaultPayslipEntryNameLengthRule(int MaxLength) : IDefaultPayslipEntryRule
{
    public List<string> Check(DefaultPayslipEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipEntryAllowedNameRule(List<string> Allowed) : IDefaultPayslipEntryRule
{
    public List<string> Check(DefaultPayslipEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipEntryRequiredNameRule(string Message) : IDefaultPayslipEntryRule
{
    public List<string> Check(DefaultPayslipEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipEntryWindowDaysRangeRule(int Min, int Max) : IDefaultPayslipEntryRule
{
    public List<string> Check(DefaultPayslipEntry item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPayslipEntryCodeLengthRule(int MaxLength) : IDefaultPayslipEntryRule
{
    public List<string> Check(DefaultPayslipEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default payslip entry.</summary>
public sealed record DefaultPayslipEntryValidator(List<IDefaultPayslipEntryRule> Rules)
{
    public List<string> Validate(DefaultPayslipEntry item)
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

    public void RequireValid(DefaultPayslipEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayslipEntry> items)
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

/// <summary>Functions of default payslip entry rules.</summary>
public static class DefaultPayslipEntryRules
{
    public static DefaultPayslipEntryValidator DefaultDefaultPayslipEntryValidator()
    {
        return new DefaultPayslipEntryValidator(
            new List<IDefaultPayslipEntryRule>
            {
                new DefaultPayslipEntryNameLengthRule(120),
                new DefaultPayslipEntryAllowedNameRule(new List<string> { "pending", "global" }),
                new DefaultPayslipEntryRequiredNameRule("default payslip entry needs this field"),
                new DefaultPayslipEntryWindowDaysRangeRule(16, 64),
                new DefaultPayslipEntryCodeLengthRule(120),
            });
    }
}
