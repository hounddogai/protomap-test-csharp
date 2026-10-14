namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding manifest must pass. Check returns its problems, or none.</summary>
public interface IOffboardingManifestRule
{
    List<string> Check(OffboardingManifest item);
}

public sealed record OffboardingManifestRequiredTitleRule(string Message) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingManifestCodeLengthRule(int MaxLength) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingManifestRequiredCodeRule(string Message) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingManifestAllowedTitleRule(List<string> Allowed) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingManifestExpiresAtRangeRule(int Min, int Max) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingManifestUnitLengthRule(int MaxLength) : IOffboardingManifestRule
{
    public List<string> Check(OffboardingManifest item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding manifest.</summary>
public sealed record OffboardingManifestValidator(List<IOffboardingManifestRule> Rules)
{
    public List<string> Validate(OffboardingManifest item)
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

    public void RequireValid(OffboardingManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingManifest> items)
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

/// <summary>Functions of offboarding manifest rules.</summary>
public static class OffboardingManifestRules
{
    public static OffboardingManifestValidator DefaultOffboardingManifestValidator()
    {
        return new OffboardingManifestValidator(
            new List<IOffboardingManifestRule>
            {
                new OffboardingManifestRequiredTitleRule("offboarding manifest needs this field"),
                new OffboardingManifestCodeLengthRule(1000),
                new OffboardingManifestRequiredCodeRule("offboarding manifest needs a value"),
                new OffboardingManifestAllowedTitleRule(new List<string> { "manual", "seasonal" }),
                new OffboardingManifestExpiresAtRangeRule(16, 255),
                new OffboardingManifestUnitLengthRule(16),
            });
    }
}
