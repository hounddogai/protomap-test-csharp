namespace Acme.Workforce.Contractors.Engagement.Score;

/// <summary>A rule a default engagement budget must pass. Check returns its problems, or none.</summary>
public interface IDefaultEngagementBudgetRule
{
    List<string> Check(DefaultEngagementBudget item);
}

public sealed record DefaultEngagementBudgetAttemptsRangeRule(int Min, int Max) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultEngagementBudgetRequiredLabelRule(string Message) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementBudgetPhoneLengthRule(int MaxLength) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementBudgetRequiredCodeRule(string Message) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementBudgetGraceDaysRangeRule(int Min, int Max) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementBudgetCostRangeRule(double Min, double Max) : IDefaultEngagementBudgetRule
{
    public List<string> Check(DefaultEngagementBudget item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default engagement budget.</summary>
public sealed record DefaultEngagementBudgetValidator(List<IDefaultEngagementBudgetRule> Rules)
{
    public List<string> Validate(DefaultEngagementBudget item)
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

    public void RequireValid(DefaultEngagementBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultEngagementBudget> items)
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

/// <summary>Functions of default engagement budget rules.</summary>
public static class DefaultEngagementBudgetRules
{
    public static DefaultEngagementBudgetValidator DefaultDefaultEngagementBudgetValidator()
    {
        return new DefaultEngagementBudgetValidator(
            new List<IDefaultEngagementBudgetRule>
            {
                new DefaultEngagementBudgetAttemptsRangeRule(80, 86400),
                new DefaultEngagementBudgetRequiredLabelRule("default engagement budget needs a value"),
                new DefaultEngagementBudgetPhoneLengthRule(16),
                new DefaultEngagementBudgetRequiredCodeRule("default engagement budget needs a value"),
                new DefaultEngagementBudgetGraceDaysRangeRule(64, 1000),
                new DefaultEngagementBudgetCostRangeRule(3.8, 3376.4),
            });
    }
}
