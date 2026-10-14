namespace Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>A rule a pending span claim must pass. Check returns its problems, or none.</summary>
public interface IPendingSpanClaimRule
{
    List<string> Check(PendingSpanClaim item);
}

public sealed record PendingSpanClaimDescriptionLengthRule(int MaxLength) : IPendingSpanClaimRule
{
    public List<string> Check(PendingSpanClaim item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanClaimCountryLengthRule(int MaxLength) : IPendingSpanClaimRule
{
    public List<string> Check(PendingSpanClaim item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanClaimRequiredDescriptionRule(string Message) : IPendingSpanClaimRule
{
    public List<string> Check(PendingSpanClaim item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanClaimTagsLimitRule(int MaxCount, List<string> Banned) : IPendingSpanClaimRule
{
    public List<string> Check(PendingSpanClaim item)
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

/// <summary>Runs every rule of a pending span claim.</summary>
public sealed record PendingSpanClaimValidator(List<IPendingSpanClaimRule> Rules)
{
    public List<string> Validate(PendingSpanClaim item)
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

    public void RequireValid(PendingSpanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSpanClaim> items)
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

/// <summary>Functions of pending span claim rules.</summary>
public static class PendingSpanClaimRules
{
    public static PendingSpanClaimValidator DefaultPendingSpanClaimValidator()
    {
        return new PendingSpanClaimValidator(
            new List<IPendingSpanClaimRule>
            {
                new PendingSpanClaimDescriptionLengthRule(32),
                new PendingSpanClaimCountryLengthRule(3600),
                new PendingSpanClaimRequiredDescriptionRule("pending span claim needs a value"),
                new PendingSpanClaimTagsLimitRule(16, new List<string> { "default", "primary" }),
            });
    }
}
