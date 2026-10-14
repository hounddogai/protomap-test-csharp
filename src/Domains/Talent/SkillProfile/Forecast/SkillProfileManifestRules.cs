namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile manifest must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileManifestRule
{
    List<string> Check(SkillProfileManifest item);
}

public sealed record SkillProfileManifestRequiredLabelRule(string Message) : ISkillProfileManifestRule
{
    public List<string> Check(SkillProfileManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileManifestOwnerIdLengthRule(int MaxLength) : ISkillProfileManifestRule
{
    public List<string> Check(SkillProfileManifest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileManifestTeamLengthRule(int MaxLength) : ISkillProfileManifestRule
{
    public List<string> Check(SkillProfileManifest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileManifestAllowedLabelRule(List<string> Allowed) : ISkillProfileManifestRule
{
    public List<string> Check(SkillProfileManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile manifest.</summary>
public sealed record SkillProfileManifestValidator(List<ISkillProfileManifestRule> Rules)
{
    public List<string> Validate(SkillProfileManifest item)
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

    public void RequireValid(SkillProfileManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileManifest> items)
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

/// <summary>Functions of skill profile manifest rules.</summary>
public static class SkillProfileManifestRules
{
    public static SkillProfileManifestValidator DefaultSkillProfileManifestValidator()
    {
        return new SkillProfileManifestValidator(
            new List<ISkillProfileManifestRule>
            {
                new SkillProfileManifestRequiredLabelRule("skill profile manifest needs a value"),
                new SkillProfileManifestOwnerIdLengthRule(64),
                new SkillProfileManifestTeamLengthRule(80),
                new SkillProfileManifestAllowedLabelRule(new List<string> { "pending", "tiered", "regional" }),
            });
    }
}
