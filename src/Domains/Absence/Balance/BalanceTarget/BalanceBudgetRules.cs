namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance budget must pass. Check returns its problems, or none.</summary>
public interface IBalanceBudgetRule
{
    List<string> Check(BalanceBudget item);
}

public sealed record BalanceBudgetRequiredCodeRule(string Message) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceBudgetJobCodeLengthRule(int MaxLength) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBudgetAllowedLabelRule(List<string> Allowed) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBudgetFteRangeRule(double Min, double Max) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBudgetPriorityRangeRule(int Min, int Max) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBudgetCodePrefixRule(string Prefix) : IBalanceBudgetRule
{
    public List<string> Check(BalanceBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance budget.</summary>
public sealed record BalanceBudgetValidator(List<IBalanceBudgetRule> Rules)
{
    public List<string> Validate(BalanceBudget item)
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

    public void RequireValid(BalanceBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceBudget> items)
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

/// <summary>Functions of balance budget rules.</summary>
public static class BalanceBudgetRules
{
    public static BalanceBudgetValidator DefaultBalanceBudgetValidator()
    {
        return new BalanceBudgetValidator(
            new List<IBalanceBudgetRule>
            {
                new BalanceBudgetRequiredCodeRule("balance budget needs this field"),
                new BalanceBudgetJobCodeLengthRule(255),
                new BalanceBudgetAllowedLabelRule(new List<string> { "adaptive" }),
                new BalanceBudgetFteRangeRule(1.9, 430.1),
                new BalanceBudgetPriorityRangeRule(3600, 86400),
                new BalanceBudgetCodePrefixRule("ACM"),
            });
    }
}
