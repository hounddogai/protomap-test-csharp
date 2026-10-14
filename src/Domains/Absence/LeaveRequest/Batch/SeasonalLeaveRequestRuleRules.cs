namespace Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>A rule a seasonal leave request rule must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLeaveRequestRuleRule
{
    List<string> Check(SeasonalLeaveRequestRule item);
}

public sealed record SeasonalLeaveRequestRuleCodePrefixRule(string Prefix) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestRuleRequiredLabelRule(string Message) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestRuleTagsLimitRule(int MaxCount, List<string> Banned) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
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

public sealed record SeasonalLeaveRequestRuleRevisionRangeRule(int Min, int Max) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestRuleRequiredLocaleRule(string Message) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestRuleRequiredCodeRule(string Message) : ISeasonalLeaveRequestRuleRule
{
    public List<string> Check(SeasonalLeaveRequestRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal leave request rule.</summary>
public sealed record SeasonalLeaveRequestRuleValidator(List<ISeasonalLeaveRequestRuleRule> Rules)
{
    public List<string> Validate(SeasonalLeaveRequestRule item)
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

    public void RequireValid(SeasonalLeaveRequestRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLeaveRequestRule> items)
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

/// <summary>Functions of seasonal leave request rule rules.</summary>
public static class SeasonalLeaveRequestRuleRules
{
    public static SeasonalLeaveRequestRuleValidator DefaultSeasonalLeaveRequestRuleValidator()
    {
        return new SeasonalLeaveRequestRuleValidator(
            new List<ISeasonalLeaveRequestRuleRule>
            {
                new SeasonalLeaveRequestRuleCodePrefixRule("X"),
                new SeasonalLeaveRequestRuleRequiredLabelRule("seasonal leave request rule needs a value"),
                new SeasonalLeaveRequestRuleTagsLimitRule(16, new List<string>()),
                new SeasonalLeaveRequestRuleRevisionRangeRule(32, 80),
                new SeasonalLeaveRequestRuleRequiredLocaleRule("seasonal leave request rule needs a value"),
                new SeasonalLeaveRequestRuleRequiredCodeRule("seasonal leave request rule needs an entry"),
            });
    }
}
