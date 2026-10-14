namespace Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>A rule a scheduled headcount budget must pass. Check returns its problems, or none.</summary>
public interface IScheduledHeadcountBudgetRule
{
    List<string> Check(ScheduledHeadcountBudget item);
}

public sealed record ScheduledHeadcountBudgetRequiredBadgeRule(string Message) : IScheduledHeadcountBudgetRule
{
    public List<string> Check(ScheduledHeadcountBudget item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledHeadcountBudgetNameLengthRule(int MaxLength) : IScheduledHeadcountBudgetRule
{
    public List<string> Check(ScheduledHeadcountBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledHeadcountBudgetRequiredNameRule(string Message) : IScheduledHeadcountBudgetRule
{
    public List<string> Check(ScheduledHeadcountBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled headcount budget.</summary>
public sealed record ScheduledHeadcountBudgetValidator(List<IScheduledHeadcountBudgetRule> Rules)
{
    public List<string> Validate(ScheduledHeadcountBudget item)
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

    public void RequireValid(ScheduledHeadcountBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledHeadcountBudget> items)
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

/// <summary>Functions of scheduled headcount budget rules.</summary>
public static class ScheduledHeadcountBudgetRules
{
    public static ScheduledHeadcountBudgetValidator DefaultScheduledHeadcountBudgetValidator()
    {
        return new ScheduledHeadcountBudgetValidator(
            new List<IScheduledHeadcountBudgetRule>
            {
                new ScheduledHeadcountBudgetRequiredBadgeRule("scheduled headcount budget needs this field"),
                new ScheduledHeadcountBudgetNameLengthRule(16),
                new ScheduledHeadcountBudgetRequiredNameRule("scheduled headcount budget needs a value"),
            });
    }
}
