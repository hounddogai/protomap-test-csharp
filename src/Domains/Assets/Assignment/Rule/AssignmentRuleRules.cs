namespace Acme.Workforce.Assets.Assignment.Rule;

/// <summary>A rule a assignment rule must pass. Check returns its problems, or none.</summary>
public interface IAssignmentRuleRule
{
    List<string> Check(AssignmentRule item);
}

public sealed record AssignmentRuleTitleLengthRule(int MaxLength) : IAssignmentRuleRule
{
    public List<string> Check(AssignmentRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRuleCodePrefixRule(string Prefix) : IAssignmentRuleRule
{
    public List<string> Check(AssignmentRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRuleBudgetRangeRule(double Min, double Max) : IAssignmentRuleRule
{
    public List<string> Check(AssignmentRule item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentRuleRequiredTitleRule(string Message) : IAssignmentRuleRule
{
    public List<string> Check(AssignmentRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment rule.</summary>
public sealed record AssignmentRuleValidator(List<IAssignmentRuleRule> Rules)
{
    public List<string> Validate(AssignmentRule item)
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

    public void RequireValid(AssignmentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentRule> items)
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

/// <summary>Functions of assignment rule rules.</summary>
public static class AssignmentRuleRules
{
    public static AssignmentRuleValidator DefaultAssignmentRuleValidator()
    {
        return new AssignmentRuleValidator(
            new List<IAssignmentRuleRule>
            {
                new AssignmentRuleTitleLengthRule(86400),
                new AssignmentRuleCodePrefixRule("REF"),
                new AssignmentRuleBudgetRangeRule(6.1, 4807.7),
                new AssignmentRuleRequiredTitleRule("assignment rule needs an entry"),
            });
    }
}
