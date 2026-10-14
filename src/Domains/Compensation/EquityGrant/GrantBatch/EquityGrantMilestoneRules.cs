namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant milestone must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantMilestoneRule
{
    List<string> Check(EquityGrantMilestone item);
}

public sealed record EquityGrantMilestoneGradeLengthRule(int MaxLength) : IEquityGrantMilestoneRule
{
    public List<string> Check(EquityGrantMilestone item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMilestoneCodePrefixRule(string Prefix) : IEquityGrantMilestoneRule
{
    public List<string> Check(EquityGrantMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMilestoneAllowedGradeRule(List<string> Allowed) : IEquityGrantMilestoneRule
{
    public List<string> Check(EquityGrantMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMilestoneRequiredTitleRule(string Message) : IEquityGrantMilestoneRule
{
    public List<string> Check(EquityGrantMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant milestone.</summary>
public sealed record EquityGrantMilestoneValidator(List<IEquityGrantMilestoneRule> Rules)
{
    public List<string> Validate(EquityGrantMilestone item)
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

    public void RequireValid(EquityGrantMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantMilestone> items)
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

/// <summary>Functions of equity grant milestone rules.</summary>
public static class EquityGrantMilestoneRules
{
    public static EquityGrantMilestoneValidator DefaultEquityGrantMilestoneValidator()
    {
        return new EquityGrantMilestoneValidator(
            new List<IEquityGrantMilestoneRule>
            {
                new EquityGrantMilestoneGradeLengthRule(1000),
                new EquityGrantMilestoneCodePrefixRule("ACM"),
                new EquityGrantMilestoneAllowedGradeRule(new List<string> { "pending", "pending", "fallback" }),
                new EquityGrantMilestoneRequiredTitleRule("equity grant milestone needs a value"),
            });
    }
}
