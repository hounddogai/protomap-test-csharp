namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement template must pass. Check returns its problems, or none.</summary>
public interface IEngagementTemplateRule
{
    List<string> Check(EngagementTemplate item);
}

public sealed record EngagementTemplateGraceDaysRangeRule(int Min, int Max) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementTemplateAttemptsRangeRule(int Min, int Max) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTemplateRetriesRangeRule(int Min, int Max) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTemplateAllowedSummaryRule(List<string> Allowed) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTemplateCodeLengthRule(int MaxLength) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTemplateRequiredCodeRule(string Message) : IEngagementTemplateRule
{
    public List<string> Check(EngagementTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement template.</summary>
public sealed record EngagementTemplateValidator(List<IEngagementTemplateRule> Rules)
{
    public List<string> Validate(EngagementTemplate item)
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

    public void RequireValid(EngagementTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementTemplate> items)
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

/// <summary>Functions of engagement template rules.</summary>
public static class EngagementTemplateRules
{
    public static EngagementTemplateValidator DefaultEngagementTemplateValidator()
    {
        return new EngagementTemplateValidator(
            new List<IEngagementTemplateRule>
            {
                new EngagementTemplateGraceDaysRangeRule(120, 120),
                new EngagementTemplateAttemptsRangeRule(16, 32),
                new EngagementTemplateRetriesRangeRule(120, 86400),
                new EngagementTemplateAllowedSummaryRule(new List<string> { "manual", "scheduled", "rolling" }),
                new EngagementTemplateCodeLengthRule(1000),
                new EngagementTemplateRequiredCodeRule("engagement template needs an entry"),
            });
    }
}
