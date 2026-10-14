namespace Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>A rule a visitor bundle must pass. Check returns its problems, or none.</summary>
public interface IVisitorBundleRule
{
    List<string> Check(VisitorBundle item);
}

public sealed record VisitorBundleCodeLengthRule(int MaxLength) : IVisitorBundleRule
{
    public List<string> Check(VisitorBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorBundleRequiredNameRule(string Message) : IVisitorBundleRule
{
    public List<string> Check(VisitorBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorBundleTagsLimitRule(int MaxCount, List<string> Banned) : IVisitorBundleRule
{
    public List<string> Check(VisitorBundle item)
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

public sealed record VisitorBundleTitleLengthRule(int MaxLength) : IVisitorBundleRule
{
    public List<string> Check(VisitorBundle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor bundle.</summary>
public sealed record VisitorBundleValidator(List<IVisitorBundleRule> Rules)
{
    public List<string> Validate(VisitorBundle item)
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

    public void RequireValid(VisitorBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorBundle> items)
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

/// <summary>Functions of visitor bundle rules.</summary>
public static class VisitorBundleRules
{
    public static VisitorBundleValidator DefaultVisitorBundleValidator()
    {
        return new VisitorBundleValidator(
            new List<IVisitorBundleRule>
            {
                new VisitorBundleCodeLengthRule(64),
                new VisitorBundleRequiredNameRule("visitor bundle needs an entry"),
                new VisitorBundleTagsLimitRule(3600, new List<string> { "strict", "pending", "lenient" }),
                new VisitorBundleTitleLengthRule(255),
            });
    }
}
