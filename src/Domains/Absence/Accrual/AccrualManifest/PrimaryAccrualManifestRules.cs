namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>A rule a primary accrual manifest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccrualManifestRule
{
    List<string> Check(PrimaryAccrualManifest item);
}

public sealed record PrimaryAccrualManifestRequiredCodeRule(string Message) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualManifestRequiredTitleRule(string Message) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualManifestTitleLengthRule(int MaxLength) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualManifestCodePrefixRule(string Prefix) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualManifestCodeLengthRule(int MaxLength) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualManifestArchivedGuardRule(double Limit) : IPrimaryAccrualManifestRule
{
    public List<string> Check(PrimaryAccrualManifest item)
    {
        if (item.Archived && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary accrual manifest.</summary>
public sealed record PrimaryAccrualManifestValidator(List<IPrimaryAccrualManifestRule> Rules)
{
    public List<string> Validate(PrimaryAccrualManifest item)
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

    public void RequireValid(PrimaryAccrualManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccrualManifest> items)
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

/// <summary>Functions of primary accrual manifest rules.</summary>
public static class PrimaryAccrualManifestRules
{
    public static PrimaryAccrualManifestValidator DefaultPrimaryAccrualManifestValidator()
    {
        return new PrimaryAccrualManifestValidator(
            new List<IPrimaryAccrualManifestRule>
            {
                new PrimaryAccrualManifestRequiredCodeRule("primary accrual manifest needs a value"),
                new PrimaryAccrualManifestRequiredTitleRule("primary accrual manifest needs an entry"),
                new PrimaryAccrualManifestTitleLengthRule(86400),
                new PrimaryAccrualManifestCodePrefixRule("OPS"),
                new PrimaryAccrualManifestCodeLengthRule(1000),
                new PrimaryAccrualManifestArchivedGuardRule(5.2),
            });
    }
}
