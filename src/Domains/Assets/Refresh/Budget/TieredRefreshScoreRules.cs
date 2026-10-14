namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered refresh score must pass. Check returns its problems, or none.</summary>
public interface ITieredRefreshScoreRule
{
    List<string> Check(TieredRefreshScore item);
}

public sealed record TieredRefreshScoreRequiredLabelRule(string Message) : ITieredRefreshScoreRule
{
    public List<string> Check(TieredRefreshScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRefreshScoreAllowedLabelRule(List<string> Allowed) : ITieredRefreshScoreRule
{
    public List<string> Check(TieredRefreshScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record TieredRefreshScoreRequiredNameRule(string Message) : ITieredRefreshScoreRule
{
    public List<string> Check(TieredRefreshScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRefreshScoreQuantityRangeRule(int Min, int Max) : ITieredRefreshScoreRule
{
    public List<string> Check(TieredRefreshScore item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered refresh score.</summary>
public sealed record TieredRefreshScoreValidator(List<ITieredRefreshScoreRule> Rules)
{
    public List<string> Validate(TieredRefreshScore item)
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

    public void RequireValid(TieredRefreshScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredRefreshScore> items)
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

/// <summary>Functions of tiered refresh score rules.</summary>
public static class TieredRefreshScoreRules
{
    public static TieredRefreshScoreValidator DefaultTieredRefreshScoreValidator()
    {
        return new TieredRefreshScoreValidator(
            new List<ITieredRefreshScoreRule>
            {
                new TieredRefreshScoreRequiredLabelRule("tiered refresh score needs a value"),
                new TieredRefreshScoreAllowedLabelRule(new List<string> { "adaptive", "lenient", "capped" }),
                new TieredRefreshScoreRequiredNameRule("tiered refresh score needs an entry"),
                new TieredRefreshScoreQuantityRangeRule(16, 120),
            });
    }
}
