namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional refresh request must pass. Check returns its problems, or none.</summary>
public interface IRegionalRefreshRequestRule
{
    List<string> Check(RegionalRefreshRequest item);
}

public sealed record RegionalRefreshRequestAllowedLabelRule(List<string> Allowed) : IRegionalRefreshRequestRule
{
    public List<string> Check(RegionalRefreshRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRefreshRequestRequiredCodeRule(string Message) : IRegionalRefreshRequestRule
{
    public List<string> Check(RegionalRefreshRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalRefreshRequestExpiresAtRangeRule(int Min, int Max) : IRegionalRefreshRequestRule
{
    public List<string> Check(RegionalRefreshRequest item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalRefreshRequestWindowDaysRangeRule(int Min, int Max) : IRegionalRefreshRequestRule
{
    public List<string> Check(RegionalRefreshRequest item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional refresh request.</summary>
public sealed record RegionalRefreshRequestValidator(List<IRegionalRefreshRequestRule> Rules)
{
    public List<string> Validate(RegionalRefreshRequest item)
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

    public void RequireValid(RegionalRefreshRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalRefreshRequest> items)
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

/// <summary>Functions of regional refresh request rules.</summary>
public static class RegionalRefreshRequestRules
{
    public static RegionalRefreshRequestValidator DefaultRegionalRefreshRequestValidator()
    {
        return new RegionalRefreshRequestValidator(
            new List<IRegionalRefreshRequestRule>
            {
                new RegionalRefreshRequestAllowedLabelRule(new List<string>()),
                new RegionalRefreshRequestRequiredCodeRule("regional refresh request needs an entry"),
                new RegionalRefreshRequestExpiresAtRangeRule(80, 120),
                new RegionalRefreshRequestWindowDaysRangeRule(80, 120),
            });
    }
}
