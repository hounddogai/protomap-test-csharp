namespace Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>A rule a legacy recognition budget must pass. Check returns its problems, or none.</summary>
public interface ILegacyRecognitionBudgetRule
{
    List<string> Check(LegacyRecognitionBudget item);
}

public sealed record LegacyRecognitionBudgetTagsLimitRule(int MaxCount, List<string> Banned) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LegacyRecognitionBudgetRequiredTeamRule(string Message) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBudgetNameLengthRule(int MaxLength) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBudgetCodePrefixRule(string Prefix) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBudgetReviewerLengthRule(int MaxLength) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBudgetTeamLengthRule(int MaxLength) : ILegacyRecognitionBudgetRule
{
    public List<string> Check(LegacyRecognitionBudget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy recognition budget.</summary>
public sealed record LegacyRecognitionBudgetValidator(List<ILegacyRecognitionBudgetRule> Rules)
{
    public List<string> Validate(LegacyRecognitionBudget item)
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

    public void RequireValid(LegacyRecognitionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRecognitionBudget> items)
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

/// <summary>Functions of legacy recognition budget rules.</summary>
public static class LegacyRecognitionBudgetRules
{
    public static LegacyRecognitionBudgetValidator DefaultLegacyRecognitionBudgetValidator()
    {
        return new LegacyRecognitionBudgetValidator(
            new List<ILegacyRecognitionBudgetRule>
            {
                new LegacyRecognitionBudgetTagsLimitRule(64, new List<string> { "lenient", "tiered" }),
                new LegacyRecognitionBudgetRequiredTeamRule("legacy recognition budget needs a value"),
                new LegacyRecognitionBudgetNameLengthRule(16),
                new LegacyRecognitionBudgetCodePrefixRule("REF"),
                new LegacyRecognitionBudgetReviewerLengthRule(32),
                new LegacyRecognitionBudgetTeamLengthRule(255),
            });
    }
}
