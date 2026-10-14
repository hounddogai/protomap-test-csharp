namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling equity grant digest must pass. Check returns its problems, or none.</summary>
public interface IRollingEquityGrantDigestRule
{
    List<string> Check(RollingEquityGrantDigest item);
}

public sealed record RollingEquityGrantDigestRequiredSummaryRule(string Message) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantDigestSummaryLengthRule(int MaxLength) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantDigestRequiredCurrencyRule(string Message) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantDigestAllowedLabelRule(List<string> Allowed) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantDigestLeaveDaysRangeRule(int Min, int Max) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
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

public sealed record RollingEquityGrantDigestCodePrefixRule(string Prefix) : IRollingEquityGrantDigestRule
{
    public List<string> Check(RollingEquityGrantDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling equity grant digest.</summary>
public sealed record RollingEquityGrantDigestValidator(List<IRollingEquityGrantDigestRule> Rules)
{
    public List<string> Validate(RollingEquityGrantDigest item)
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

    public void RequireValid(RollingEquityGrantDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingEquityGrantDigest> items)
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

/// <summary>Functions of rolling equity grant digest rules.</summary>
public static class RollingEquityGrantDigestRules
{
    public static RollingEquityGrantDigestValidator DefaultRollingEquityGrantDigestValidator()
    {
        return new RollingEquityGrantDigestValidator(
            new List<IRollingEquityGrantDigestRule>
            {
                new RollingEquityGrantDigestRequiredSummaryRule("rolling equity grant digest needs a value"),
                new RollingEquityGrantDigestSummaryLengthRule(64),
                new RollingEquityGrantDigestRequiredCurrencyRule("rolling equity grant digest needs an entry"),
                new RollingEquityGrantDigestAllowedLabelRule(new List<string> { "regional", "manual" }),
                new RollingEquityGrantDigestLeaveDaysRangeRule(80, 1000),
                new RollingEquityGrantDigestCodePrefixRule("ACM"),
            });
    }
}
