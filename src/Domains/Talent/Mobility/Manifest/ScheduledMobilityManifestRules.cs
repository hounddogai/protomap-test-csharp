namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled mobility manifest must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityManifestRule
{
    List<string> Check(ScheduledMobilityManifest item);
}

public sealed record ScheduledMobilityManifestGradeLengthRule(int MaxLength) : IScheduledMobilityManifestRule
{
    public List<string> Check(ScheduledMobilityManifest item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityManifestAllowedDescriptionRule(List<string> Allowed) : IScheduledMobilityManifestRule
{
    public List<string> Check(ScheduledMobilityManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityManifestDescriptionLengthRule(int MaxLength) : IScheduledMobilityManifestRule
{
    public List<string> Check(ScheduledMobilityManifest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityManifestCodeLengthRule(int MaxLength) : IScheduledMobilityManifestRule
{
    public List<string> Check(ScheduledMobilityManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility manifest.</summary>
public sealed record ScheduledMobilityManifestValidator(List<IScheduledMobilityManifestRule> Rules)
{
    public List<string> Validate(ScheduledMobilityManifest item)
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

    public void RequireValid(ScheduledMobilityManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityManifest> items)
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

/// <summary>Functions of scheduled mobility manifest rules.</summary>
public static class ScheduledMobilityManifestRules
{
    public static ScheduledMobilityManifestValidator DefaultScheduledMobilityManifestValidator()
    {
        return new ScheduledMobilityManifestValidator(
            new List<IScheduledMobilityManifestRule>
            {
                new ScheduledMobilityManifestGradeLengthRule(1000),
                new ScheduledMobilityManifestAllowedDescriptionRule(new List<string>()),
                new ScheduledMobilityManifestDescriptionLengthRule(1000),
                new ScheduledMobilityManifestCodeLengthRule(255),
            });
    }
}
