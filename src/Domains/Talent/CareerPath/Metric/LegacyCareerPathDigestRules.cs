namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy career path digest must pass. Check returns its problems, or none.</summary>
public interface ILegacyCareerPathDigestRule
{
    List<string> Check(LegacyCareerPathDigest item);
}

public sealed record LegacyCareerPathDigestCodeLengthRule(int MaxLength) : ILegacyCareerPathDigestRule
{
    public List<string> Check(LegacyCareerPathDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathDigestAllowedCodeRule(List<string> Allowed) : ILegacyCareerPathDigestRule
{
    public List<string> Check(LegacyCareerPathDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathDigestContactEmailLengthRule(int MaxLength) : ILegacyCareerPathDigestRule
{
    public List<string> Check(LegacyCareerPathDigest item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathDigestTitleLengthRule(int MaxLength) : ILegacyCareerPathDigestRule
{
    public List<string> Check(LegacyCareerPathDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy career path digest.</summary>
public sealed record LegacyCareerPathDigestValidator(List<ILegacyCareerPathDigestRule> Rules)
{
    public List<string> Validate(LegacyCareerPathDigest item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LegacyCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyCareerPathDigest> items)
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

/// <summary>Functions of legacy career path digest rules.</summary>
public static class LegacyCareerPathDigestRules
{
    public static LegacyCareerPathDigestValidator DefaultLegacyCareerPathDigestValidator()
    {
        return new LegacyCareerPathDigestValidator(
            new List<ILegacyCareerPathDigestRule>
            {
                new LegacyCareerPathDigestCodeLengthRule(16),
                new LegacyCareerPathDigestAllowedCodeRule(new List<string> { "tiered", "manual", "manual" }),
                new LegacyCareerPathDigestContactEmailLengthRule(3600),
                new LegacyCareerPathDigestTitleLengthRule(86400),
            });
    }
}
