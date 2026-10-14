namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>A rule a capped access badge manifest must pass. Check returns its problems, or none.</summary>
public interface ICappedAccessBadgeManifestRule
{
    List<string> Check(CappedAccessBadgeManifest item);
}

public sealed record CappedAccessBadgeManifestCodeLengthRule(int MaxLength) : ICappedAccessBadgeManifestRule
{
    public List<string> Check(CappedAccessBadgeManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeManifestTagsLimitRule(int MaxCount, List<string> Banned) : ICappedAccessBadgeManifestRule
{
    public List<string> Check(CappedAccessBadgeManifest item)
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

public sealed record CappedAccessBadgeManifestRequiredCodeRule(string Message) : ICappedAccessBadgeManifestRule
{
    public List<string> Check(CappedAccessBadgeManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeManifestRequiredCategoryRule(string Message) : ICappedAccessBadgeManifestRule
{
    public List<string> Check(CappedAccessBadgeManifest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeManifestPhoneLengthRule(int MaxLength) : ICappedAccessBadgeManifestRule
{
    public List<string> Check(CappedAccessBadgeManifest item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped access badge manifest.</summary>
public sealed record CappedAccessBadgeManifestValidator(List<ICappedAccessBadgeManifestRule> Rules)
{
    public List<string> Validate(CappedAccessBadgeManifest item)
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

    public void RequireValid(CappedAccessBadgeManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccessBadgeManifest> items)
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

/// <summary>Functions of capped access badge manifest rules.</summary>
public static class CappedAccessBadgeManifestRules
{
    public static CappedAccessBadgeManifestValidator DefaultCappedAccessBadgeManifestValidator()
    {
        return new CappedAccessBadgeManifestValidator(
            new List<ICappedAccessBadgeManifestRule>
            {
                new CappedAccessBadgeManifestCodeLengthRule(64),
                new CappedAccessBadgeManifestTagsLimitRule(16, new List<string> { "regional", "primary" }),
                new CappedAccessBadgeManifestRequiredCodeRule("capped access badge manifest needs a value"),
                new CappedAccessBadgeManifestRequiredCategoryRule("capped access badge manifest needs an entry"),
                new CappedAccessBadgeManifestPhoneLengthRule(120),
            });
    }
}
