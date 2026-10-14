namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>A rule a tiered merit cycle plan must pass. Check returns its problems, or none.</summary>
public interface ITieredMeritCyclePlanRule
{
    List<string> Check(TieredMeritCyclePlan item);
}

public sealed record TieredMeritCyclePlanLimitRangeRule(int Min, int Max) : ITieredMeritCyclePlanRule
{
    public List<string> Check(TieredMeritCyclePlan item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredMeritCyclePlanWeightRangeRule(double Min, double Max) : ITieredMeritCyclePlanRule
{
    public List<string> Check(TieredMeritCyclePlan item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMeritCyclePlanRequiredManagerIdRule(string Message) : ITieredMeritCyclePlanRule
{
    public List<string> Check(TieredMeritCyclePlan item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered merit cycle plan.</summary>
public sealed record TieredMeritCyclePlanValidator(List<ITieredMeritCyclePlanRule> Rules)
{
    public List<string> Validate(TieredMeritCyclePlan item)
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

    public void RequireValid(TieredMeritCyclePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredMeritCyclePlan> items)
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

/// <summary>Functions of tiered merit cycle plan rules.</summary>
public static class TieredMeritCyclePlanRules
{
    public static TieredMeritCyclePlanValidator DefaultTieredMeritCyclePlanValidator()
    {
        return new TieredMeritCyclePlanValidator(
            new List<ITieredMeritCyclePlanRule>
            {
                new TieredMeritCyclePlanLimitRangeRule(32, 32),
                new TieredMeritCyclePlanWeightRangeRule(0.6, 309.7),
                new TieredMeritCyclePlanRequiredManagerIdRule("tiered merit cycle plan needs a value"),
            });
    }
}
