namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered occupancy rule must pass. Check returns its problems, or none.</summary>
public interface ITieredOccupancyRuleRule
{
    List<string> Check(TieredOccupancyRule item);
}

public sealed record TieredOccupancyRuleTenureMonthsRangeRule(int Min, int Max) : ITieredOccupancyRuleRule
{
    public List<string> Check(TieredOccupancyRule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredOccupancyRuleRequiredCodeRule(string Message) : ITieredOccupancyRuleRule
{
    public List<string> Check(TieredOccupancyRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOccupancyRuleAllowedCodeRule(List<string> Allowed) : ITieredOccupancyRuleRule
{
    public List<string> Check(TieredOccupancyRule item)
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

/// <summary>Runs every rule of a tiered occupancy rule.</summary>
public sealed record TieredOccupancyRuleValidator(List<ITieredOccupancyRuleRule> Rules)
{
    public List<string> Validate(TieredOccupancyRule item)
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

    public void RequireValid(TieredOccupancyRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredOccupancyRule> items)
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

/// <summary>Functions of tiered occupancy rule rules.</summary>
public static class TieredOccupancyRuleRules
{
    public static TieredOccupancyRuleValidator DefaultTieredOccupancyRuleValidator()
    {
        return new TieredOccupancyRuleValidator(
            new List<ITieredOccupancyRuleRule>
            {
                new TieredOccupancyRuleTenureMonthsRangeRule(32, 3600),
                new TieredOccupancyRuleRequiredCodeRule("tiered occupancy rule needs a value"),
                new TieredOccupancyRuleAllowedCodeRule(new List<string> { "regional", "legacy" }),
            });
    }
}
