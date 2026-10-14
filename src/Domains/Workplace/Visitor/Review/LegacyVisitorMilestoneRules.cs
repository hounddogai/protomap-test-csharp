namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy visitor milestone must pass. Check returns its problems, or none.</summary>
public interface ILegacyVisitorMilestoneRule
{
    List<string> Check(LegacyVisitorMilestone item);
}

public sealed record LegacyVisitorMilestoneNameLengthRule(int MaxLength) : ILegacyVisitorMilestoneRule
{
    public List<string> Check(LegacyVisitorMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVisitorMilestoneActiveGuardRule(int Limit) : ILegacyVisitorMilestoneRule
{
    public List<string> Check(LegacyVisitorMilestone item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVisitorMilestoneAllowedCodeRule(List<string> Allowed) : ILegacyVisitorMilestoneRule
{
    public List<string> Check(LegacyVisitorMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy visitor milestone.</summary>
public sealed record LegacyVisitorMilestoneValidator(List<ILegacyVisitorMilestoneRule> Rules)
{
    public List<string> Validate(LegacyVisitorMilestone item)
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

    public void RequireValid(LegacyVisitorMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyVisitorMilestone> items)
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

/// <summary>Functions of legacy visitor milestone rules.</summary>
public static class LegacyVisitorMilestoneRules
{
    public static LegacyVisitorMilestoneValidator DefaultLegacyVisitorMilestoneValidator()
    {
        return new LegacyVisitorMilestoneValidator(
            new List<ILegacyVisitorMilestoneRule>
            {
                new LegacyVisitorMilestoneNameLengthRule(3600),
                new LegacyVisitorMilestoneActiveGuardRule(1000),
                new LegacyVisitorMilestoneAllowedCodeRule(new List<string> { "pending" }),
            });
    }
}
