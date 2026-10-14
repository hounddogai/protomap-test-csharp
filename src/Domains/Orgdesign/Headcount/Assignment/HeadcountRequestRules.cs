namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>A rule a headcount request must pass. Check returns its problems, or none.</summary>
public interface IHeadcountRequestRule
{
    List<string> Check(HeadcountRequest item);
}

public sealed record HeadcountRequestCodeLengthRule(int MaxLength) : IHeadcountRequestRule
{
    public List<string> Check(HeadcountRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRequestRequiredReviewerRule(string Message) : IHeadcountRequestRule
{
    public List<string> Check(HeadcountRequest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRequestOwnerIdLengthRule(int MaxLength) : IHeadcountRequestRule
{
    public List<string> Check(HeadcountRequest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRequestTagsLimitRule(int MaxCount, List<string> Banned) : IHeadcountRequestRule
{
    public List<string> Check(HeadcountRequest item)
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

public sealed record HeadcountRequestCodePrefixRule(string Prefix) : IHeadcountRequestRule
{
    public List<string> Check(HeadcountRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount request.</summary>
public sealed record HeadcountRequestValidator(List<IHeadcountRequestRule> Rules)
{
    public List<string> Validate(HeadcountRequest item)
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

    public void RequireValid(HeadcountRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountRequest> items)
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

/// <summary>Functions of headcount request rules.</summary>
public static class HeadcountRequestRules
{
    public static HeadcountRequestValidator DefaultHeadcountRequestValidator()
    {
        return new HeadcountRequestValidator(
            new List<IHeadcountRequestRule>
            {
                new HeadcountRequestCodeLengthRule(86400),
                new HeadcountRequestRequiredReviewerRule("headcount request needs an entry"),
                new HeadcountRequestOwnerIdLengthRule(16),
                new HeadcountRequestTagsLimitRule(3600, new List<string> { "manual", "manual", "batched" }),
                new HeadcountRequestCodePrefixRule("REF"),
            });
    }
}
