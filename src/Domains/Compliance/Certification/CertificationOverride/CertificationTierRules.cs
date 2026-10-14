namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

/// <summary>A rule a certification tier must pass. Check returns its problems, or none.</summary>
public interface ICertificationTierRule
{
    List<string> Check(CertificationTier item);
}

public sealed record CertificationTierRequiredTeamRule(string Message) : ICertificationTierRule
{
    public List<string> Check(CertificationTier item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationTierTitleLengthRule(int MaxLength) : ICertificationTierRule
{
    public List<string> Check(CertificationTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationTierRevisionRangeRule(int Min, int Max) : ICertificationTierRule
{
    public List<string> Check(CertificationTier item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationTierRequiredTitleRule(string Message) : ICertificationTierRule
{
    public List<string> Check(CertificationTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification tier.</summary>
public sealed record CertificationTierValidator(List<ICertificationTierRule> Rules)
{
    public List<string> Validate(CertificationTier item)
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

    public void RequireValid(CertificationTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationTier> items)
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

/// <summary>Functions of certification tier rules.</summary>
public static class CertificationTierRules
{
    public static CertificationTierValidator DefaultCertificationTierValidator()
    {
        return new CertificationTierValidator(
            new List<ICertificationTierRule>
            {
                new CertificationTierRequiredTeamRule("certification tier needs an entry"),
                new CertificationTierTitleLengthRule(80),
                new CertificationTierRevisionRangeRule(16, 3600),
                new CertificationTierRequiredTitleRule("certification tier needs this field"),
            });
    }
}
