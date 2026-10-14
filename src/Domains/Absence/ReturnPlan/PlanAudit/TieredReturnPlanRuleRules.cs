namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>A rule a tiered return plan rule must pass. Check returns its problems, or none.</summary>
public interface ITieredReturnPlanRuleRule
{
    List<string> Check(TieredReturnPlanRule item);
}

public sealed record TieredReturnPlanRuleRequiredNameRule(string Message) : ITieredReturnPlanRuleRule
{
    public List<string> Check(TieredReturnPlanRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanRuleCodeLengthRule(int MaxLength) : ITieredReturnPlanRuleRule
{
    public List<string> Check(TieredReturnPlanRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanRuleRequiredTeamRule(string Message) : ITieredReturnPlanRuleRule
{
    public List<string> Check(TieredReturnPlanRule item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanRuleScoreRangeRule(double Min, double Max) : ITieredReturnPlanRuleRule
{
    public List<string> Check(TieredReturnPlanRule item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredReturnPlanRuleTeamLengthRule(int MaxLength) : ITieredReturnPlanRuleRule
{
    public List<string> Check(TieredReturnPlanRule item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered return plan rule.</summary>
public sealed record TieredReturnPlanRuleValidator(List<ITieredReturnPlanRuleRule> Rules)
{
    public List<string> Validate(TieredReturnPlanRule item)
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

    public void RequireValid(TieredReturnPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredReturnPlanRule> items)
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

/// <summary>Functions of tiered return plan rule rules.</summary>
public static class TieredReturnPlanRuleRules
{
    public static TieredReturnPlanRuleValidator DefaultTieredReturnPlanRuleValidator()
    {
        return new TieredReturnPlanRuleValidator(
            new List<ITieredReturnPlanRuleRule>
            {
                new TieredReturnPlanRuleRequiredNameRule("tiered return plan rule needs this field"),
                new TieredReturnPlanRuleCodeLengthRule(1000),
                new TieredReturnPlanRuleRequiredTeamRule("tiered return plan rule needs an entry"),
                new TieredReturnPlanRuleScoreRangeRule(4.6, 3777.8),
                new TieredReturnPlanRuleTeamLengthRule(64),
            });
    }
}
