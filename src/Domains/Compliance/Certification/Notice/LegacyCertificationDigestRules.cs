namespace Acme.Workforce.Compliance.Certification.Notice;

/// <summary>A rule a legacy certification digest must pass. Check returns its problems, or none.</summary>
public interface ILegacyCertificationDigestRule
{
    List<string> Check(LegacyCertificationDigest item);
}

public sealed record LegacyCertificationDigestCurrencyLengthRule(int MaxLength) : ILegacyCertificationDigestRule
{
    public List<string> Check(LegacyCertificationDigest item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCertificationDigestRequiredGradeRule(string Message) : ILegacyCertificationDigestRule
{
    public List<string> Check(LegacyCertificationDigest item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyCertificationDigestNameLengthRule(int MaxLength) : ILegacyCertificationDigestRule
{
    public List<string> Check(LegacyCertificationDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCertificationDigestRequiredNameRule(string Message) : ILegacyCertificationDigestRule
{
    public List<string> Check(LegacyCertificationDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy certification digest.</summary>
public sealed record LegacyCertificationDigestValidator(List<ILegacyCertificationDigestRule> Rules)
{
    public List<string> Validate(LegacyCertificationDigest item)
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

    public void RequireValid(LegacyCertificationDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyCertificationDigest> items)
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

/// <summary>Functions of legacy certification digest rules.</summary>
public static class LegacyCertificationDigestRules
{
    public static LegacyCertificationDigestValidator DefaultLegacyCertificationDigestValidator()
    {
        return new LegacyCertificationDigestValidator(
            new List<ILegacyCertificationDigestRule>
            {
                new LegacyCertificationDigestCurrencyLengthRule(1000),
                new LegacyCertificationDigestRequiredGradeRule("legacy certification digest needs a value"),
                new LegacyCertificationDigestNameLengthRule(80),
                new LegacyCertificationDigestRequiredNameRule("legacy certification digest needs this field"),
            });
    }
}
