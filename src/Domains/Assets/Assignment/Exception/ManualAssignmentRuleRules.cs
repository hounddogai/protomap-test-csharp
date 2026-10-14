namespace Acme.Workforce.Assets.Assignment.Exception;

/// <summary>A rule a manual assignment rule must pass. Check returns its problems, or none.</summary>
public interface IManualAssignmentRuleRule
{
    List<string> Check(ManualAssignmentRule item);
}

public sealed record ManualAssignmentRuleScoreRangeRule(double Min, double Max) : IManualAssignmentRuleRule
{
    public List<string> Check(ManualAssignmentRule item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentRuleLocationLengthRule(int MaxLength) : IManualAssignmentRuleRule
{
    public List<string> Check(ManualAssignmentRule item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentRuleRequiredChannelRule(string Message) : IManualAssignmentRuleRule
{
    public List<string> Check(ManualAssignmentRule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentRuleTagsLimitRule(int MaxCount, List<string> Banned) : IManualAssignmentRuleRule
{
    public List<string> Check(ManualAssignmentRule item)
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

/// <summary>Runs every rule of a manual assignment rule.</summary>
public sealed record ManualAssignmentRuleValidator(List<IManualAssignmentRuleRule> Rules)
{
    public List<string> Validate(ManualAssignmentRule item)
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

    public void RequireValid(ManualAssignmentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAssignmentRule> items)
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

/// <summary>Functions of manual assignment rule rules.</summary>
public static class ManualAssignmentRuleRules
{
    public static ManualAssignmentRuleValidator DefaultManualAssignmentRuleValidator()
    {
        return new ManualAssignmentRuleValidator(
            new List<IManualAssignmentRuleRule>
            {
                new ManualAssignmentRuleScoreRangeRule(7.3, 4783.7),
                new ManualAssignmentRuleLocationLengthRule(3600),
                new ManualAssignmentRuleRequiredChannelRule("manual assignment rule needs an entry"),
                new ManualAssignmentRuleTagsLimitRule(86400, new List<string> { "primary" }),
            });
    }
}
