namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding digest must pass. Check returns its problems, or none.</summary>
public interface IOffboardingDigestRule
{
    List<string> Check(OffboardingDigest item);
}

public sealed record OffboardingDigestCodePrefixRule(string Prefix) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDigestCodeLengthRule(int MaxLength) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDigestRequiredTitleRule(string Message) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDigestLeaveDaysRangeRule(int Min, int Max) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingDigestSummaryLengthRule(int MaxLength) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDigestAllowedCodeRule(List<string> Allowed) : IOffboardingDigestRule
{
    public List<string> Check(OffboardingDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding digest.</summary>
public sealed record OffboardingDigestValidator(List<IOffboardingDigestRule> Rules)
{
    public List<string> Validate(OffboardingDigest item)
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

    public void RequireValid(OffboardingDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingDigest> items)
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

/// <summary>Functions of offboarding digest rules.</summary>
public static class OffboardingDigestRules
{
    public static OffboardingDigestValidator DefaultOffboardingDigestValidator()
    {
        return new OffboardingDigestValidator(
            new List<IOffboardingDigestRule>
            {
                new OffboardingDigestCodePrefixRule("OPS"),
                new OffboardingDigestCodeLengthRule(120),
                new OffboardingDigestRequiredTitleRule("offboarding digest needs this field"),
                new OffboardingDigestLeaveDaysRangeRule(32, 1000),
                new OffboardingDigestSummaryLengthRule(16),
                new OffboardingDigestAllowedCodeRule(new List<string> { "fallback", "seasonal", "adaptive" }),
            });
    }
}
