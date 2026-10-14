namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped offboarding baseline must pass. Check returns its problems, or none.</summary>
public interface ICappedOffboardingBaselineRule
{
    List<string> Check(CappedOffboardingBaseline item);
}

public sealed record CappedOffboardingBaselineAllowedChecksumRule(List<string> Allowed) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is too long" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingBaselineRequiredTitleRule(string Message) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingBaselineAmountRangeRule(double Min, double Max) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedOffboardingBaselineCodeLengthRule(int MaxLength) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingBaselineCodePrefixRule(string Prefix) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingBaselineTitleLengthRule(int MaxLength) : ICappedOffboardingBaselineRule
{
    public List<string> Check(CappedOffboardingBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped offboarding baseline.</summary>
public sealed record CappedOffboardingBaselineValidator(List<ICappedOffboardingBaselineRule> Rules)
{
    public List<string> Validate(CappedOffboardingBaseline item)
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

    public void RequireValid(CappedOffboardingBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOffboardingBaseline> items)
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

/// <summary>Functions of capped offboarding baseline rules.</summary>
public static class CappedOffboardingBaselineRules
{
    public static CappedOffboardingBaselineValidator DefaultCappedOffboardingBaselineValidator()
    {
        return new CappedOffboardingBaselineValidator(
            new List<ICappedOffboardingBaselineRule>
            {
                new CappedOffboardingBaselineAllowedChecksumRule(new List<string>()),
                new CappedOffboardingBaselineRequiredTitleRule("capped offboarding baseline needs this field"),
                new CappedOffboardingBaselineAmountRangeRule(4.6, 374.5),
                new CappedOffboardingBaselineCodeLengthRule(255),
                new CappedOffboardingBaselineCodePrefixRule("OPS"),
                new CappedOffboardingBaselineTitleLengthRule(255),
            });
    }
}
