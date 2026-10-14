namespace Acme.Workforce.Compliance.Exception.Profile;

/// <summary>A rule a pending exception manifest must pass. Check returns its problems, or none.</summary>
public interface IPendingExceptionManifestRule
{
    List<string> Check(PendingExceptionManifest item);
}

public sealed record PendingExceptionManifestPhoneLengthRule(int MaxLength) : IPendingExceptionManifestRule
{
    public List<string> Check(PendingExceptionManifest item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionManifestRequiredCodeRule(string Message) : IPendingExceptionManifestRule
{
    public List<string> Check(PendingExceptionManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionManifestCodePrefixRule(string Prefix) : IPendingExceptionManifestRule
{
    public List<string> Check(PendingExceptionManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending exception manifest.</summary>
public sealed record PendingExceptionManifestValidator(List<IPendingExceptionManifestRule> Rules)
{
    public List<string> Validate(PendingExceptionManifest item)
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

    public void RequireValid(PendingExceptionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingExceptionManifest> items)
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

/// <summary>Functions of pending exception manifest rules.</summary>
public static class PendingExceptionManifestRules
{
    public static PendingExceptionManifestValidator DefaultPendingExceptionManifestValidator()
    {
        return new PendingExceptionManifestValidator(
            new List<IPendingExceptionManifestRule>
            {
                new PendingExceptionManifestPhoneLengthRule(64),
                new PendingExceptionManifestRequiredCodeRule("pending exception manifest needs a value"),
                new PendingExceptionManifestCodePrefixRule("ACM"),
            });
    }
}
