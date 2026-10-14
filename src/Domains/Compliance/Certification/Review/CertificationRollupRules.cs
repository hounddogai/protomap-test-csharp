namespace Acme.Workforce.Compliance.Certification.Review;

/// <summary>A rule a certification rollup must pass. Check returns its problems, or none.</summary>
public interface ICertificationRollupRule
{
    List<string> Check(CertificationRollup item);
}

public sealed record CertificationRollupLabelLengthRule(int MaxLength) : ICertificationRollupRule
{
    public List<string> Check(CertificationRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationRollupCodePrefixRule(string Prefix) : ICertificationRollupRule
{
    public List<string> Check(CertificationRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationRollupEndsAtRangeRule(int Min, int Max) : ICertificationRollupRule
{
    public List<string> Check(CertificationRollup item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification rollup.</summary>
public sealed record CertificationRollupValidator(List<ICertificationRollupRule> Rules)
{
    public List<string> Validate(CertificationRollup item)
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

    public void RequireValid(CertificationRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationRollup> items)
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

/// <summary>Functions of certification rollup rules.</summary>
public static class CertificationRollupRules
{
    public static CertificationRollupValidator DefaultCertificationRollupValidator()
    {
        return new CertificationRollupValidator(
            new List<ICertificationRollupRule>
            {
                new CertificationRollupLabelLengthRule(255),
                new CertificationRollupCodePrefixRule("ACM"),
                new CertificationRollupEndsAtRangeRule(32, 120),
            });
    }
}
