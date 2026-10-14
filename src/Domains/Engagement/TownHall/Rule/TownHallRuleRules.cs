namespace Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>A rule a town hall rule must pass. Check returns its problems, or none.</summary>
public interface ITownHallRuleRule
{
    List<string> Check(TownHallRule item);
}

public sealed record TownHallRuleRequiredNameRule(string Message) : ITownHallRuleRule
{
    public List<string> Check(TownHallRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallRuleRequiredJobCodeRule(string Message) : ITownHallRuleRule
{
    public List<string> Check(TownHallRule item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallRuleRatioRangeRule(double Min, double Max) : ITownHallRuleRule
{
    public List<string> Check(TownHallRule item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall rule.</summary>
public sealed record TownHallRuleValidator(List<ITownHallRuleRule> Rules)
{
    public List<string> Validate(TownHallRule item)
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

    public void RequireValid(TownHallRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallRule> items)
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

/// <summary>Functions of town hall rule rules.</summary>
public static class TownHallRuleRules
{
    public static TownHallRuleValidator DefaultTownHallRuleValidator()
    {
        return new TownHallRuleValidator(
            new List<ITownHallRuleRule>
            {
                new TownHallRuleRequiredNameRule("town hall rule needs a value"),
                new TownHallRuleRequiredJobCodeRule("town hall rule needs this field"),
                new TownHallRuleRatioRangeRule(8.2, 2942.3),
            });
    }
}
