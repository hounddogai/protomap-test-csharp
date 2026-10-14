namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling rate variance must pass. Check returns its problems, or none.</summary>
public interface IRollingRateVarianceRule
{
    List<string> Check(RollingRateVariance item);
}

public sealed record RollingRateVarianceRequiredCountryRule(string Message) : IRollingRateVarianceRule
{
    public List<string> Check(RollingRateVariance item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRateVarianceCountryLengthRule(int MaxLength) : IRollingRateVarianceRule
{
    public List<string> Check(RollingRateVariance item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateVarianceRequiredReviewerRule(string Message) : IRollingRateVarianceRule
{
    public List<string> Check(RollingRateVariance item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRateVarianceCodePrefixRule(string Prefix) : IRollingRateVarianceRule
{
    public List<string> Check(RollingRateVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateVarianceAllowedCodeRule(List<string> Allowed) : IRollingRateVarianceRule
{
    public List<string> Check(RollingRateVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling rate variance.</summary>
public sealed record RollingRateVarianceValidator(List<IRollingRateVarianceRule> Rules)
{
    public List<string> Validate(RollingRateVariance item)
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

    public void RequireValid(RollingRateVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRateVariance> items)
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

/// <summary>Functions of rolling rate variance rules.</summary>
public static class RollingRateVarianceRules
{
    public static RollingRateVarianceValidator DefaultRollingRateVarianceValidator()
    {
        return new RollingRateVarianceValidator(
            new List<IRollingRateVarianceRule>
            {
                new RollingRateVarianceRequiredCountryRule("rolling rate variance needs this field"),
                new RollingRateVarianceCountryLengthRule(3600),
                new RollingRateVarianceRequiredReviewerRule("rolling rate variance needs an entry"),
                new RollingRateVarianceCodePrefixRule("Q"),
                new RollingRateVarianceAllowedCodeRule(new List<string> { "strict" }),
            });
    }
}
