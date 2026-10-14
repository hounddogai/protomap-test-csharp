namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default payslip budget must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayslipBudgetRule
{
    List<string> Check(DefaultPayslipBudget item);
}

public sealed record DefaultPayslipBudgetRequiredCodeRule(string Message) : IDefaultPayslipBudgetRule
{
    public List<string> Check(DefaultPayslipBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipBudgetAllowedTitleRule(List<string> Allowed) : IDefaultPayslipBudgetRule
{
    public List<string> Check(DefaultPayslipBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipBudgetCodePrefixRule(string Prefix) : IDefaultPayslipBudgetRule
{
    public List<string> Check(DefaultPayslipBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipBudgetRevisionRangeRule(int Min, int Max) : IDefaultPayslipBudgetRule
{
    public List<string> Check(DefaultPayslipBudget item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipBudgetStartsAtRangeRule(int Min, int Max) : IDefaultPayslipBudgetRule
{
    public List<string> Check(DefaultPayslipBudget item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default payslip budget.</summary>
public sealed record DefaultPayslipBudgetValidator(List<IDefaultPayslipBudgetRule> Rules)
{
    public List<string> Validate(DefaultPayslipBudget item)
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

    public void RequireValid(DefaultPayslipBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayslipBudget> items)
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

/// <summary>Functions of default payslip budget rules.</summary>
public static class DefaultPayslipBudgetRules
{
    public static DefaultPayslipBudgetValidator DefaultDefaultPayslipBudgetValidator()
    {
        return new DefaultPayslipBudgetValidator(
            new List<IDefaultPayslipBudgetRule>
            {
                new DefaultPayslipBudgetRequiredCodeRule("default payslip budget needs a value"),
                new DefaultPayslipBudgetAllowedTitleRule(new List<string> { "global", "batched" }),
                new DefaultPayslipBudgetCodePrefixRule("X"),
                new DefaultPayslipBudgetRevisionRangeRule(120, 120),
                new DefaultPayslipBudgetStartsAtRangeRule(64, 120),
            });
    }
}
