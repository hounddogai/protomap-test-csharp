namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh request must pass. Check returns its problems, or none.</summary>
public interface IRefreshRequestRule
{
    List<string> Check(RefreshRequest item);
}

public sealed record RefreshRequestCodeLengthRule(int MaxLength) : IRefreshRequestRule
{
    public List<string> Check(RefreshRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRequestAllowedTitleRule(List<string> Allowed) : IRefreshRequestRule
{
    public List<string> Check(RefreshRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRequestRequiredTitleRule(string Message) : IRefreshRequestRule
{
    public List<string> Check(RefreshRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshRequestCodePrefixRule(string Prefix) : IRefreshRequestRule
{
    public List<string> Check(RefreshRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRequestLabelLengthRule(int MaxLength) : IRefreshRequestRule
{
    public List<string> Check(RefreshRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh request.</summary>
public sealed record RefreshRequestValidator(List<IRefreshRequestRule> Rules)
{
    public List<string> Validate(RefreshRequest item)
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

    public void RequireValid(RefreshRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshRequest> items)
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

/// <summary>Functions of refresh request rules.</summary>
public static class RefreshRequestRules
{
    public static RefreshRequestValidator DefaultRefreshRequestValidator()
    {
        return new RefreshRequestValidator(
            new List<IRefreshRequestRule>
            {
                new RefreshRequestCodeLengthRule(32),
                new RefreshRequestAllowedTitleRule(new List<string>()),
                new RefreshRequestRequiredTitleRule("refresh request needs this field"),
                new RefreshRequestCodePrefixRule("ACM"),
                new RefreshRequestLabelLengthRule(1000),
            });
    }
}
