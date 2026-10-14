namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy town hall score must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallScoreRule
{
    List<string> Check(LegacyTownHallScore item);
}

public sealed record LegacyTownHallScoreAllowedNameRule(List<string> Allowed) : ILegacyTownHallScoreRule
{
    public List<string> Check(LegacyTownHallScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScoreRequiredCodeRule(string Message) : ILegacyTownHallScoreRule
{
    public List<string> Check(LegacyTownHallScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScoreCodeLengthRule(int MaxLength) : ILegacyTownHallScoreRule
{
    public List<string> Check(LegacyTownHallScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallScoreRequiredExternalRefRule(string Message) : ILegacyTownHallScoreRule
{
    public List<string> Check(LegacyTownHallScore item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall score.</summary>
public sealed record LegacyTownHallScoreValidator(List<ILegacyTownHallScoreRule> Rules)
{
    public List<string> Validate(LegacyTownHallScore item)
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

    public void RequireValid(LegacyTownHallScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallScore> items)
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

/// <summary>Functions of legacy town hall score rules.</summary>
public static class LegacyTownHallScoreRules
{
    public static LegacyTownHallScoreValidator DefaultLegacyTownHallScoreValidator()
    {
        return new LegacyTownHallScoreValidator(
            new List<ILegacyTownHallScoreRule>
            {
                new LegacyTownHallScoreAllowedNameRule(new List<string> { "legacy", "rolling" }),
                new LegacyTownHallScoreRequiredCodeRule("legacy town hall score needs a value"),
                new LegacyTownHallScoreCodeLengthRule(80),
                new LegacyTownHallScoreRequiredExternalRefRule("legacy town hall score needs this field"),
            });
    }
}
