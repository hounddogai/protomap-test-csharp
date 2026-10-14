namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh digest must pass. Check returns its problems, or none.</summary>
public interface IRefreshDigestRule
{
    List<string> Check(RefreshDigest item);
}

public sealed record RefreshDigestAllowedReviewerRule(List<string> Allowed) : IRefreshDigestRule
{
    public List<string> Check(RefreshDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RefreshDigestCodePrefixRule(string Prefix) : IRefreshDigestRule
{
    public List<string> Check(RefreshDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshDigestWeightRangeRule(double Min, double Max) : IRefreshDigestRule
{
    public List<string> Check(RefreshDigest item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshDigestRequiredTitleRule(string Message) : IRefreshDigestRule
{
    public List<string> Check(RefreshDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshDigestCodeLengthRule(int MaxLength) : IRefreshDigestRule
{
    public List<string> Check(RefreshDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh digest.</summary>
public sealed record RefreshDigestValidator(List<IRefreshDigestRule> Rules)
{
    public List<string> Validate(RefreshDigest item)
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

    public void RequireValid(RefreshDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshDigest> items)
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

/// <summary>Functions of refresh digest rules.</summary>
public static class RefreshDigestRules
{
    public static RefreshDigestValidator DefaultRefreshDigestValidator()
    {
        return new RefreshDigestValidator(
            new List<IRefreshDigestRule>
            {
                new RefreshDigestAllowedReviewerRule(new List<string> { "rolling", "global" }),
                new RefreshDigestCodePrefixRule("Q"),
                new RefreshDigestWeightRangeRule(4.1, 3026.0),
                new RefreshDigestRequiredTitleRule("refresh digest needs a value"),
                new RefreshDigestCodeLengthRule(16),
            });
    }
}
