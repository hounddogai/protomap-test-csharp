namespace Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>A rule a strict career path digest must pass. Check returns its problems, or none.</summary>
public interface IStrictCareerPathDigestRule
{
    List<string> Check(StrictCareerPathDigest item);
}

public sealed record StrictCareerPathDigestCodeLengthRule(int MaxLength) : IStrictCareerPathDigestRule
{
    public List<string> Check(StrictCareerPathDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathDigestRequiredChecksumRule(string Message) : IStrictCareerPathDigestRule
{
    public List<string> Check(StrictCareerPathDigest item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathDigestNameLengthRule(int MaxLength) : IStrictCareerPathDigestRule
{
    public List<string> Check(StrictCareerPathDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathDigestChecksumLengthRule(int MaxLength) : IStrictCareerPathDigestRule
{
    public List<string> Check(StrictCareerPathDigest item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathDigestRequiredCountryRule(string Message) : IStrictCareerPathDigestRule
{
    public List<string> Check(StrictCareerPathDigest item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict career path digest.</summary>
public sealed record StrictCareerPathDigestValidator(List<IStrictCareerPathDigestRule> Rules)
{
    public List<string> Validate(StrictCareerPathDigest item)
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

    public void RequireValid(StrictCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictCareerPathDigest> items)
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

/// <summary>Functions of strict career path digest rules.</summary>
public static class StrictCareerPathDigestRules
{
    public static StrictCareerPathDigestValidator DefaultStrictCareerPathDigestValidator()
    {
        return new StrictCareerPathDigestValidator(
            new List<IStrictCareerPathDigestRule>
            {
                new StrictCareerPathDigestCodeLengthRule(32),
                new StrictCareerPathDigestRequiredChecksumRule("strict career path digest needs this field"),
                new StrictCareerPathDigestNameLengthRule(86400),
                new StrictCareerPathDigestChecksumLengthRule(16),
                new StrictCareerPathDigestRequiredCountryRule("strict career path digest needs this field"),
            });
    }
}
