namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending exception quota must pass. Check returns its problems, or none.</summary>
public interface IPendingExceptionQuotaRule
{
    List<string> Check(PendingExceptionQuota item);
}

public sealed record PendingExceptionQuotaVisibleGuardRule(int Limit) : IPendingExceptionQuotaRule
{
    public List<string> Check(PendingExceptionQuota item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionQuotaCodeLengthRule(int MaxLength) : IPendingExceptionQuotaRule
{
    public List<string> Check(PendingExceptionQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionQuotaAllowedNameRule(List<string> Allowed) : IPendingExceptionQuotaRule
{
    public List<string> Check(PendingExceptionQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionQuotaTagsLimitRule(int MaxCount, List<string> Banned) : IPendingExceptionQuotaRule
{
    public List<string> Check(PendingExceptionQuota item)
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

/// <summary>Runs every rule of a pending exception quota.</summary>
public sealed record PendingExceptionQuotaValidator(List<IPendingExceptionQuotaRule> Rules)
{
    public List<string> Validate(PendingExceptionQuota item)
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

    public void RequireValid(PendingExceptionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingExceptionQuota> items)
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

/// <summary>Functions of pending exception quota rules.</summary>
public static class PendingExceptionQuotaRules
{
    public static PendingExceptionQuotaValidator DefaultPendingExceptionQuotaValidator()
    {
        return new PendingExceptionQuotaValidator(
            new List<IPendingExceptionQuotaRule>
            {
                new PendingExceptionQuotaVisibleGuardRule(3600),
                new PendingExceptionQuotaCodeLengthRule(255),
                new PendingExceptionQuotaAllowedNameRule(new List<string>()),
                new PendingExceptionQuotaTagsLimitRule(86400, new List<string> { "lenient", "tiered", "pending" }),
            });
    }
}
