namespace Acme.Workforce.Contractors.Engagement.Template;

/// <summary>A rule a legacy engagement budget must pass. Check returns its problems, or none.</summary>
public interface ILegacyEngagementBudgetRule
{
    List<string> Check(LegacyEngagementBudget item);
}

public sealed record LegacyEngagementBudgetSequenceRangeRule(int Min, int Max) : ILegacyEngagementBudgetRule
{
    public List<string> Check(LegacyEngagementBudget item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyEngagementBudgetRequiredLocationRule(string Message) : ILegacyEngagementBudgetRule
{
    public List<string> Check(LegacyEngagementBudget item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyEngagementBudgetNameLengthRule(int MaxLength) : ILegacyEngagementBudgetRule
{
    public List<string> Check(LegacyEngagementBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy engagement budget.</summary>
public sealed record LegacyEngagementBudgetValidator(List<ILegacyEngagementBudgetRule> Rules)
{
    public List<string> Validate(LegacyEngagementBudget item)
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

    public void RequireValid(LegacyEngagementBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyEngagementBudget> items)
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

/// <summary>Functions of legacy engagement budget rules.</summary>
public static class LegacyEngagementBudgetRules
{
    public static LegacyEngagementBudgetValidator DefaultLegacyEngagementBudgetValidator()
    {
        return new LegacyEngagementBudgetValidator(
            new List<ILegacyEngagementBudgetRule>
            {
                new LegacyEngagementBudgetSequenceRangeRule(255, 1000),
                new LegacyEngagementBudgetRequiredLocationRule("legacy engagement budget needs this field"),
                new LegacyEngagementBudgetNameLengthRule(1000),
            });
    }
}
