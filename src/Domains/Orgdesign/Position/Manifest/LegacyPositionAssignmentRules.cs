namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy position assignment must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionAssignmentRule
{
    List<string> Check(LegacyPositionAssignment item);
}

public sealed record LegacyPositionAssignmentArchivedGuardRule(int Limit) : ILegacyPositionAssignmentRule
{
    public List<string> Check(LegacyPositionAssignment item)
    {
        if (item.Archived && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionAssignmentAllowedSkillRule(List<string> Allowed) : ILegacyPositionAssignmentRule
{
    public List<string> Check(LegacyPositionAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionAssignmentRequiredSkillRule(string Message) : ILegacyPositionAssignmentRule
{
    public List<string> Check(LegacyPositionAssignment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionAssignmentRequiredTitleRule(string Message) : ILegacyPositionAssignmentRule
{
    public List<string> Check(LegacyPositionAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position assignment.</summary>
public sealed record LegacyPositionAssignmentValidator(List<ILegacyPositionAssignmentRule> Rules)
{
    public List<string> Validate(LegacyPositionAssignment item)
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

    public void RequireValid(LegacyPositionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionAssignment> items)
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

/// <summary>Functions of legacy position assignment rules.</summary>
public static class LegacyPositionAssignmentRules
{
    public static LegacyPositionAssignmentValidator DefaultLegacyPositionAssignmentValidator()
    {
        return new LegacyPositionAssignmentValidator(
            new List<ILegacyPositionAssignmentRule>
            {
                new LegacyPositionAssignmentArchivedGuardRule(3600),
                new LegacyPositionAssignmentAllowedSkillRule(new List<string> { "tiered", "legacy" }),
                new LegacyPositionAssignmentRequiredSkillRule("legacy position assignment needs an entry"),
                new LegacyPositionAssignmentRequiredTitleRule("legacy position assignment needs this field"),
            });
    }
}
