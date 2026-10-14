namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary floor plan bundle must pass. Check returns its problems, or none.</summary>
public interface IPrimaryFloorPlanBundleRule
{
    List<string> Check(PrimaryFloorPlanBundle item);
}

public sealed record PrimaryFloorPlanBundleChecksumLengthRule(int MaxLength) : IPrimaryFloorPlanBundleRule
{
    public List<string> Check(PrimaryFloorPlanBundle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanBundleLabelLengthRule(int MaxLength) : IPrimaryFloorPlanBundleRule
{
    public List<string> Check(PrimaryFloorPlanBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanBundleTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryFloorPlanBundleRule
{
    public List<string> Check(PrimaryFloorPlanBundle item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryFloorPlanBundleAllowedTimezoneRule(List<string> Allowed) : IPrimaryFloorPlanBundleRule
{
    public List<string> Check(PrimaryFloorPlanBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary floor plan bundle.</summary>
public sealed record PrimaryFloorPlanBundleValidator(List<IPrimaryFloorPlanBundleRule> Rules)
{
    public List<string> Validate(PrimaryFloorPlanBundle item)
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

    public void RequireValid(PrimaryFloorPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryFloorPlanBundle> items)
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

/// <summary>Functions of primary floor plan bundle rules.</summary>
public static class PrimaryFloorPlanBundleRules
{
    public static PrimaryFloorPlanBundleValidator DefaultPrimaryFloorPlanBundleValidator()
    {
        return new PrimaryFloorPlanBundleValidator(
            new List<IPrimaryFloorPlanBundleRule>
            {
                new PrimaryFloorPlanBundleChecksumLengthRule(255),
                new PrimaryFloorPlanBundleLabelLengthRule(64),
                new PrimaryFloorPlanBundleTagsLimitRule(255, new List<string> { "tiered" }),
                new PrimaryFloorPlanBundleAllowedTimezoneRule(new List<string> { "seasonal" }),
            });
    }
}
