namespace Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>A rule a refresh window must pass. Check returns its problems, or none.</summary>
public interface IRefreshWindowRule
{
    List<string> Check(RefreshWindow item);
}

public sealed record RefreshWindowRequiredLocaleRule(string Message) : IRefreshWindowRule
{
    public List<string> Check(RefreshWindow item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshWindowRequiredOwnerIdRule(string Message) : IRefreshWindowRule
{
    public List<string> Check(RefreshWindow item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshWindowCodePrefixRule(string Prefix) : IRefreshWindowRule
{
    public List<string> Check(RefreshWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh window.</summary>
public sealed record RefreshWindowValidator(List<IRefreshWindowRule> Rules)
{
    public List<string> Validate(RefreshWindow item)
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

    public void RequireValid(RefreshWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshWindow> items)
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

/// <summary>Functions of refresh window rules.</summary>
public static class RefreshWindowRules
{
    public static RefreshWindowValidator DefaultRefreshWindowValidator()
    {
        return new RefreshWindowValidator(
            new List<IRefreshWindowRule>
            {
                new RefreshWindowRequiredLocaleRule("refresh window needs an entry"),
                new RefreshWindowRequiredOwnerIdRule("refresh window needs this field"),
                new RefreshWindowCodePrefixRule("ACM"),
            });
    }
}
