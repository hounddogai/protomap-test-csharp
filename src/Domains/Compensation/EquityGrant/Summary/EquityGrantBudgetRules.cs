namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant budget must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantBudgetRule
{
    List<string> Check(EquityGrantBudget item);
}

public sealed record EquityGrantBudgetRequiredSalaryBandRule(string Message) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBudgetOwnerIdLengthRule(int MaxLength) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBudgetSalaryBandLengthRule(int MaxLength) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBudgetRequiredCodeRule(string Message) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBudgetCodePrefixRule(string Prefix) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBudgetAllowedCodeRule(List<string> Allowed) : IEquityGrantBudgetRule
{
    public List<string> Check(EquityGrantBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant budget.</summary>
public sealed record EquityGrantBudgetValidator(List<IEquityGrantBudgetRule> Rules)
{
    public List<string> Validate(EquityGrantBudget item)
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

    public void RequireValid(EquityGrantBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantBudget> items)
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

/// <summary>Functions of equity grant budget rules.</summary>
public static class EquityGrantBudgetRules
{
    public static EquityGrantBudgetValidator DefaultEquityGrantBudgetValidator()
    {
        return new EquityGrantBudgetValidator(
            new List<IEquityGrantBudgetRule>
            {
                new EquityGrantBudgetRequiredSalaryBandRule("equity grant budget needs a value"),
                new EquityGrantBudgetOwnerIdLengthRule(3600),
                new EquityGrantBudgetSalaryBandLengthRule(32),
                new EquityGrantBudgetRequiredCodeRule("equity grant budget needs this field"),
                new EquityGrantBudgetCodePrefixRule("ACM"),
                new EquityGrantBudgetAllowedCodeRule(new List<string> { "capped", "weighted" }),
            });
    }
}
