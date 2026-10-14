namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>A rule a rolling bonus pool template must pass. Check returns its problems, or none.</summary>
public interface IRollingBonusPoolTemplateRule
{
    List<string> Check(RollingBonusPoolTemplate item);
}

public sealed record RollingBonusPoolTemplateLabelLengthRule(int MaxLength) : IRollingBonusPoolTemplateRule
{
    public List<string> Check(RollingBonusPoolTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolTemplateCodePrefixRule(string Prefix) : IRollingBonusPoolTemplateRule
{
    public List<string> Check(RollingBonusPoolTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolTemplateRequiredCodeRule(string Message) : IRollingBonusPoolTemplateRule
{
    public List<string> Check(RollingBonusPoolTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolTemplateDurationMinutesRangeRule(int Min, int Max) : IRollingBonusPoolTemplateRule
{
    public List<string> Check(RollingBonusPoolTemplate item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBonusPoolTemplateReviewerLengthRule(int MaxLength) : IRollingBonusPoolTemplateRule
{
    public List<string> Check(RollingBonusPoolTemplate item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling bonus pool template.</summary>
public sealed record RollingBonusPoolTemplateValidator(List<IRollingBonusPoolTemplateRule> Rules)
{
    public List<string> Validate(RollingBonusPoolTemplate item)
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

    public void RequireValid(RollingBonusPoolTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBonusPoolTemplate> items)
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

/// <summary>Functions of rolling bonus pool template rules.</summary>
public static class RollingBonusPoolTemplateRules
{
    public static RollingBonusPoolTemplateValidator DefaultRollingBonusPoolTemplateValidator()
    {
        return new RollingBonusPoolTemplateValidator(
            new List<IRollingBonusPoolTemplateRule>
            {
                new RollingBonusPoolTemplateLabelLengthRule(80),
                new RollingBonusPoolTemplateCodePrefixRule("ACM"),
                new RollingBonusPoolTemplateRequiredCodeRule("rolling bonus pool template needs this field"),
                new RollingBonusPoolTemplateDurationMinutesRangeRule(64, 3600),
                new RollingBonusPoolTemplateReviewerLengthRule(86400),
            });
    }
}
