namespace Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>A rule a engagement budget must pass. Check returns its problems, or none.</summary>
public interface IEngagementBudgetRule
{
    List<string> Check(EngagementBudget item);
}

public sealed record EngagementBudgetCodeLengthRule(int MaxLength) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBudgetManagerIdLengthRule(int MaxLength) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBudgetRequiredCodeRule(string Message) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBudgetRequiredTitleRule(string Message) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBudgetRequiredManagerIdRule(string Message) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBudgetSequenceRangeRule(int Min, int Max) : IEngagementBudgetRule
{
    public List<string> Check(EngagementBudget item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement budget.</summary>
public sealed record EngagementBudgetValidator(List<IEngagementBudgetRule> Rules)
{
    public List<string> Validate(EngagementBudget item)
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

    public void RequireValid(EngagementBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementBudget> items)
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

/// <summary>Functions of engagement budget rules.</summary>
public static class EngagementBudgetRules
{
    public static EngagementBudgetValidator DefaultEngagementBudgetValidator()
    {
        return new EngagementBudgetValidator(
            new List<IEngagementBudgetRule>
            {
                new EngagementBudgetCodeLengthRule(86400),
                new EngagementBudgetManagerIdLengthRule(86400),
                new EngagementBudgetRequiredCodeRule("engagement budget needs an entry"),
                new EngagementBudgetRequiredTitleRule("engagement budget needs a value"),
                new EngagementBudgetRequiredManagerIdRule("engagement budget needs a value"),
                new EngagementBudgetSequenceRangeRule(16, 32),
            });
    }
}
