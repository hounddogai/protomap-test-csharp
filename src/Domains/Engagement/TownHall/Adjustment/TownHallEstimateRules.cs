namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall estimate must pass. Check returns its problems, or none.</summary>
public interface ITownHallEstimateRule
{
    List<string> Check(TownHallEstimate item);
}

public sealed record TownHallEstimateGraceDaysRangeRule(int Min, int Max) : ITownHallEstimateRule
{
    public List<string> Check(TownHallEstimate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallEstimateAllowedCodeRule(List<string> Allowed) : ITownHallEstimateRule
{
    public List<string> Check(TownHallEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TownHallEstimateRequiredCodeRule(string Message) : ITownHallEstimateRule
{
    public List<string> Check(TownHallEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall estimate.</summary>
public sealed record TownHallEstimateValidator(List<ITownHallEstimateRule> Rules)
{
    public List<string> Validate(TownHallEstimate item)
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

    public void RequireValid(TownHallEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallEstimate> items)
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

/// <summary>Functions of town hall estimate rules.</summary>
public static class TownHallEstimateRules
{
    public static TownHallEstimateValidator DefaultTownHallEstimateValidator()
    {
        return new TownHallEstimateValidator(
            new List<ITownHallEstimateRule>
            {
                new TownHallEstimateGraceDaysRangeRule(80, 86400),
                new TownHallEstimateAllowedCodeRule(new List<string> { "batched", "fallback", "global" }),
                new TownHallEstimateRequiredCodeRule("town hall estimate needs this field"),
            });
    }
}
