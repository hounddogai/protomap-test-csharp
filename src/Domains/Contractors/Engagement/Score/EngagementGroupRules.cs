namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement group must pass. Check returns its problems, or none.</summary>
public interface IEngagementGroupRule
{
    List<string> Check(EngagementGroup item);
}

public sealed record EngagementGroupCodeLengthRule(int MaxLength) : IEngagementGroupRule
{
    public List<string> Check(EngagementGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementGroupActiveGuardRule(int Limit) : IEngagementGroupRule
{
    public List<string> Check(EngagementGroup item)
    {
        if (item.Active && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record EngagementGroupRequiredCodeRule(string Message) : IEngagementGroupRule
{
    public List<string> Check(EngagementGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementGroupAllowedCodeRule(List<string> Allowed) : IEngagementGroupRule
{
    public List<string> Check(EngagementGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement group.</summary>
public sealed record EngagementGroupValidator(List<IEngagementGroupRule> Rules)
{
    public List<string> Validate(EngagementGroup item)
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

    public void RequireValid(EngagementGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementGroup> items)
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

/// <summary>Functions of engagement group rules.</summary>
public static class EngagementGroupRules
{
    public static EngagementGroupValidator DefaultEngagementGroupValidator()
    {
        return new EngagementGroupValidator(
            new List<IEngagementGroupRule>
            {
                new EngagementGroupCodeLengthRule(32),
                new EngagementGroupActiveGuardRule(64),
                new EngagementGroupRequiredCodeRule("engagement group needs this field"),
                new EngagementGroupAllowedCodeRule(new List<string> { "primary", "pending" }),
            });
    }
}
