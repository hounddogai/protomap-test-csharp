namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy town hall manifest must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallManifestRule
{
    List<string> Check(LegacyTownHallManifest item);
}

public sealed record LegacyTownHallManifestCreatedAtRangeRule(int Min, int Max) : ILegacyTownHallManifestRule
{
    public List<string> Check(LegacyTownHallManifest item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallManifestAllowedCodeRule(List<string> Allowed) : ILegacyTownHallManifestRule
{
    public List<string> Check(LegacyTownHallManifest item)
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

public sealed record LegacyTownHallManifestJobCodeLengthRule(int MaxLength) : ILegacyTownHallManifestRule
{
    public List<string> Check(LegacyTownHallManifest item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall manifest.</summary>
public sealed record LegacyTownHallManifestValidator(List<ILegacyTownHallManifestRule> Rules)
{
    public List<string> Validate(LegacyTownHallManifest item)
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

    public void RequireValid(LegacyTownHallManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallManifest> items)
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

/// <summary>Functions of legacy town hall manifest rules.</summary>
public static class LegacyTownHallManifestRules
{
    public static LegacyTownHallManifestValidator DefaultLegacyTownHallManifestValidator()
    {
        return new LegacyTownHallManifestValidator(
            new List<ILegacyTownHallManifestRule>
            {
                new LegacyTownHallManifestCreatedAtRangeRule(255, 1000),
                new LegacyTownHallManifestAllowedCodeRule(new List<string> { "primary" }),
                new LegacyTownHallManifestJobCodeLengthRule(80),
            });
    }
}
