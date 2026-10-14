namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback audit manifest must pass. Check returns its problems, or none.</summary>
public interface IFallbackAuditManifestRule
{
    List<string> Check(FallbackAuditManifest item);
}

public sealed record FallbackAuditManifestRequiredCodeRule(string Message) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAuditManifestAllowedLocaleRule(List<string> Allowed) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAuditManifestExpiresAtRangeRule(int Min, int Max) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAuditManifestRequiredLocaleRule(string Message) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAuditManifestCodePrefixRule(string Prefix) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAuditManifestPinnedGuardRule(int Limit) : IFallbackAuditManifestRule
{
    public List<string> Check(FallbackAuditManifest item)
    {
        if (item.Pinned && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback audit manifest.</summary>
public sealed record FallbackAuditManifestValidator(List<IFallbackAuditManifestRule> Rules)
{
    public List<string> Validate(FallbackAuditManifest item)
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

    public void RequireValid(FallbackAuditManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAuditManifest> items)
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

/// <summary>Functions of fallback audit manifest rules.</summary>
public static class FallbackAuditManifestRules
{
    public static FallbackAuditManifestValidator DefaultFallbackAuditManifestValidator()
    {
        return new FallbackAuditManifestValidator(
            new List<IFallbackAuditManifestRule>
            {
                new FallbackAuditManifestRequiredCodeRule("fallback audit manifest needs a value"),
                new FallbackAuditManifestAllowedLocaleRule(new List<string> { "default", "regional", "weighted" }),
                new FallbackAuditManifestExpiresAtRangeRule(32, 120),
                new FallbackAuditManifestRequiredLocaleRule("fallback audit manifest needs this field"),
                new FallbackAuditManifestCodePrefixRule("REF"),
                new FallbackAuditManifestPinnedGuardRule(16),
            });
    }
}
