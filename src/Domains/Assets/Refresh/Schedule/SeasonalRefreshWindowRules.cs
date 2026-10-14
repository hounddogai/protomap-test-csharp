namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal refresh window must pass. Check returns its problems, or none.</summary>
public interface ISeasonalRefreshWindowRule
{
    List<string> Check(SeasonalRefreshWindow item);
}

public sealed record SeasonalRefreshWindowCodeLengthRule(int MaxLength) : ISeasonalRefreshWindowRule
{
    public List<string> Check(SeasonalRefreshWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshWindowRequiredTitleRule(string Message) : ISeasonalRefreshWindowRule
{
    public List<string> Check(SeasonalRefreshWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshWindowAllowedCodeRule(List<string> Allowed) : ISeasonalRefreshWindowRule
{
    public List<string> Check(SeasonalRefreshWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshWindowRequiredCodeRule(string Message) : ISeasonalRefreshWindowRule
{
    public List<string> Check(SeasonalRefreshWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshWindowTitleLengthRule(int MaxLength) : ISeasonalRefreshWindowRule
{
    public List<string> Check(SeasonalRefreshWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal refresh window.</summary>
public sealed record SeasonalRefreshWindowValidator(List<ISeasonalRefreshWindowRule> Rules)
{
    public List<string> Validate(SeasonalRefreshWindow item)
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

    public void RequireValid(SeasonalRefreshWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalRefreshWindow> items)
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

/// <summary>Functions of seasonal refresh window rules.</summary>
public static class SeasonalRefreshWindowRules
{
    public static SeasonalRefreshWindowValidator DefaultSeasonalRefreshWindowValidator()
    {
        return new SeasonalRefreshWindowValidator(
            new List<ISeasonalRefreshWindowRule>
            {
                new SeasonalRefreshWindowCodeLengthRule(120),
                new SeasonalRefreshWindowRequiredTitleRule("seasonal refresh window needs this field"),
                new SeasonalRefreshWindowAllowedCodeRule(new List<string> { "default", "scheduled", "fallback" }),
                new SeasonalRefreshWindowRequiredCodeRule("seasonal refresh window needs this field"),
                new SeasonalRefreshWindowTitleLengthRule(64),
            });
    }
}
