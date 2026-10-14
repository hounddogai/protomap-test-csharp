namespace Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>A rule a strict balance budget must pass. Check returns its problems, or none.</summary>
public interface IStrictBalanceBudgetRule
{
    List<string> Check(StrictBalanceBudget item);
}

public sealed record StrictBalanceBudgetCodeLengthRule(int MaxLength) : IStrictBalanceBudgetRule
{
    public List<string> Check(StrictBalanceBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBalanceBudgetCodePrefixRule(string Prefix) : IStrictBalanceBudgetRule
{
    public List<string> Check(StrictBalanceBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBalanceBudgetFteRangeRule(double Min, double Max) : IStrictBalanceBudgetRule
{
    public List<string> Check(StrictBalanceBudget item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBalanceBudgetPriorityRangeRule(int Min, int Max) : IStrictBalanceBudgetRule
{
    public List<string> Check(StrictBalanceBudget item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict balance budget.</summary>
public sealed record StrictBalanceBudgetValidator(List<IStrictBalanceBudgetRule> Rules)
{
    public List<string> Validate(StrictBalanceBudget item)
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

    public void RequireValid(StrictBalanceBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictBalanceBudget> items)
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

/// <summary>Functions of strict balance budget rules.</summary>
public static class StrictBalanceBudgetRules
{
    public static StrictBalanceBudgetValidator DefaultStrictBalanceBudgetValidator()
    {
        return new StrictBalanceBudgetValidator(
            new List<IStrictBalanceBudgetRule>
            {
                new StrictBalanceBudgetCodeLengthRule(1000),
                new StrictBalanceBudgetCodePrefixRule("X"),
                new StrictBalanceBudgetFteRangeRule(1.4, 1093.7),
                new StrictBalanceBudgetPriorityRangeRule(120, 1000),
            });
    }
}
