namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement rollup must pass. Check returns its problems, or none.</summary>
public interface IEngagementRollupRule
{
    List<string> Check(EngagementRollup item);
}

public sealed record EngagementRollupRequiredCodeRule(string Message) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementRollupHoursRangeRule(double Min, double Max) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRollupSkillLengthRule(int MaxLength) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRollupAllowedCodeRule(List<string> Allowed) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRollupTimezoneLengthRule(int MaxLength) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRollupRequiredLabelRule(string Message) : IEngagementRollupRule
{
    public List<string> Check(EngagementRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement rollup.</summary>
public sealed record EngagementRollupValidator(List<IEngagementRollupRule> Rules)
{
    public List<string> Validate(EngagementRollup item)
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

    public void RequireValid(EngagementRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementRollup> items)
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

/// <summary>Functions of engagement rollup rules.</summary>
public static class EngagementRollupRules
{
    public static EngagementRollupValidator DefaultEngagementRollupValidator()
    {
        return new EngagementRollupValidator(
            new List<IEngagementRollupRule>
            {
                new EngagementRollupRequiredCodeRule("engagement rollup needs a value"),
                new EngagementRollupHoursRangeRule(4.6, 742.1),
                new EngagementRollupSkillLengthRule(1000),
                new EngagementRollupAllowedCodeRule(new List<string> { "weighted", "batched", "manual" }),
                new EngagementRollupTimezoneLengthRule(80),
                new EngagementRollupRequiredLabelRule("engagement rollup needs this field"),
            });
    }
}
