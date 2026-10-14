namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>A rule a tiered scenario window must pass. Check returns its problems, or none.</summary>
public interface ITieredScenarioWindowRule
{
    List<string> Check(TieredScenarioWindow item);
}

public sealed record TieredScenarioWindowWindowDaysRangeRule(int Min, int Max) : ITieredScenarioWindowRule
{
    public List<string> Check(TieredScenarioWindow item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredScenarioWindowRatioRangeRule(double Min, double Max) : ITieredScenarioWindowRule
{
    public List<string> Check(TieredScenarioWindow item)
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

public sealed record TieredScenarioWindowRequiredCodeRule(string Message) : ITieredScenarioWindowRule
{
    public List<string> Check(TieredScenarioWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredScenarioWindowRequiredUnitRule(string Message) : ITieredScenarioWindowRule
{
    public List<string> Check(TieredScenarioWindow item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered scenario window.</summary>
public sealed record TieredScenarioWindowValidator(List<ITieredScenarioWindowRule> Rules)
{
    public List<string> Validate(TieredScenarioWindow item)
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

    public void RequireValid(TieredScenarioWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredScenarioWindow> items)
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

/// <summary>Functions of tiered scenario window rules.</summary>
public static class TieredScenarioWindowRules
{
    public static TieredScenarioWindowValidator DefaultTieredScenarioWindowValidator()
    {
        return new TieredScenarioWindowValidator(
            new List<ITieredScenarioWindowRule>
            {
                new TieredScenarioWindowWindowDaysRangeRule(255, 3600),
                new TieredScenarioWindowRatioRangeRule(3.3, 4112.2),
                new TieredScenarioWindowRequiredCodeRule("tiered scenario window needs a value"),
                new TieredScenarioWindowRequiredUnitRule("tiered scenario window needs an entry"),
            });
    }
}
