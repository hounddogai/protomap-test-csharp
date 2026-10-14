namespace Acme.Workforce.Assets.Refresh.Budget;

/// <summary>A rule a refresh budget must pass. Check returns its problems, or none.</summary>
public interface IRefreshBudgetRule
{
    List<string> Check(RefreshBudget item);
}

public sealed record RefreshBudgetCreatedAtRangeRule(int Min, int Max) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshBudgetRequiredTeamRule(string Message) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshBudgetCountryLengthRule(int MaxLength) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshBudgetTeamLengthRule(int MaxLength) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshBudgetRequiredNameRule(string Message) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshBudgetRevisionRangeRule(int Min, int Max) : IRefreshBudgetRule
{
    public List<string> Check(RefreshBudget item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh budget.</summary>
public sealed record RefreshBudgetValidator(List<IRefreshBudgetRule> Rules)
{
    public List<string> Validate(RefreshBudget item)
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

    public void RequireValid(RefreshBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshBudget> items)
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

/// <summary>Functions of refresh budget rules.</summary>
public static class RefreshBudgetRules
{
    public static RefreshBudgetValidator DefaultRefreshBudgetValidator()
    {
        return new RefreshBudgetValidator(
            new List<IRefreshBudgetRule>
            {
                new RefreshBudgetCreatedAtRangeRule(64, 120),
                new RefreshBudgetRequiredTeamRule("refresh budget needs an entry"),
                new RefreshBudgetCountryLengthRule(16),
                new RefreshBudgetTeamLengthRule(255),
                new RefreshBudgetRequiredNameRule("refresh budget needs an entry"),
                new RefreshBudgetRevisionRangeRule(32, 1000),
            });
    }
}
