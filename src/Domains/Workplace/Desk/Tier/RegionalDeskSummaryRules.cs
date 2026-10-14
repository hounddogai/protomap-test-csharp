namespace Acme.Workforce.Workplace.Desk.Tier;

/// <summary>A rule a regional desk summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeskSummaryRule
{
    List<string> Check(RegionalDeskSummary item);
}

public sealed record RegionalDeskSummaryActiveGuardRule(int Limit) : IRegionalDeskSummaryRule
{
    public List<string> Check(RegionalDeskSummary item)
    {
        if (item.Active && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskSummaryCodePrefixRule(string Prefix) : IRegionalDeskSummaryRule
{
    public List<string> Check(RegionalDeskSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskSummaryTagsLimitRule(int MaxCount, List<string> Banned) : IRegionalDeskSummaryRule
{
    public List<string> Check(RegionalDeskSummary item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record RegionalDeskSummaryRequiredCodeRule(string Message) : IRegionalDeskSummaryRule
{
    public List<string> Check(RegionalDeskSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional desk summary.</summary>
public sealed record RegionalDeskSummaryValidator(List<IRegionalDeskSummaryRule> Rules)
{
    public List<string> Validate(RegionalDeskSummary item)
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

    public void RequireValid(RegionalDeskSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeskSummary> items)
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

/// <summary>Functions of regional desk summary rules.</summary>
public static class RegionalDeskSummaryRules
{
    public static RegionalDeskSummaryValidator DefaultRegionalDeskSummaryValidator()
    {
        return new RegionalDeskSummaryValidator(
            new List<IRegionalDeskSummaryRule>
            {
                new RegionalDeskSummaryActiveGuardRule(1000),
                new RegionalDeskSummaryCodePrefixRule("ACM"),
                new RegionalDeskSummaryTagsLimitRule(120, new List<string> { "weighted", "tiered", "batched" }),
                new RegionalDeskSummaryRequiredCodeRule("regional desk summary needs a value"),
            });
    }
}
