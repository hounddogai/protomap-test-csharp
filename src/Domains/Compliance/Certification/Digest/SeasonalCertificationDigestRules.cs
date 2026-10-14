namespace Acme.Workforce.Compliance.Certification.Digest;

/// <summary>A rule a seasonal certification digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCertificationDigestRule
{
    List<string> Check(SeasonalCertificationDigest item);
}

public sealed record SeasonalCertificationDigestDurationMinutesRangeRule(int Min, int Max) : ISeasonalCertificationDigestRule
{
    public List<string> Check(SeasonalCertificationDigest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationDigestRequiredCodeRule(string Message) : ISeasonalCertificationDigestRule
{
    public List<string> Check(SeasonalCertificationDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationDigestLabelLengthRule(int MaxLength) : ISeasonalCertificationDigestRule
{
    public List<string> Check(SeasonalCertificationDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCertificationDigestCodePrefixRule(string Prefix) : ISeasonalCertificationDigestRule
{
    public List<string> Check(SeasonalCertificationDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal certification digest.</summary>
public sealed record SeasonalCertificationDigestValidator(List<ISeasonalCertificationDigestRule> Rules)
{
    public List<string> Validate(SeasonalCertificationDigest item)
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

    public void RequireValid(SeasonalCertificationDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCertificationDigest> items)
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

/// <summary>Functions of seasonal certification digest rules.</summary>
public static class SeasonalCertificationDigestRules
{
    public static SeasonalCertificationDigestValidator DefaultSeasonalCertificationDigestValidator()
    {
        return new SeasonalCertificationDigestValidator(
            new List<ISeasonalCertificationDigestRule>
            {
                new SeasonalCertificationDigestDurationMinutesRangeRule(32, 64),
                new SeasonalCertificationDigestRequiredCodeRule("seasonal certification digest needs this field"),
                new SeasonalCertificationDigestLabelLengthRule(16),
                new SeasonalCertificationDigestCodePrefixRule("Q"),
            });
    }
}
