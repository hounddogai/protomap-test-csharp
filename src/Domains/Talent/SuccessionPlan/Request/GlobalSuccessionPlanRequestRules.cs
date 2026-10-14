namespace Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>A rule a global succession plan request must pass. Check returns its problems, or none.</summary>
public interface IGlobalSuccessionPlanRequestRule
{
    List<string> Check(GlobalSuccessionPlanRequest item);
}

public sealed record GlobalSuccessionPlanRequestLabelLengthRule(int MaxLength) : IGlobalSuccessionPlanRequestRule
{
    public List<string> Check(GlobalSuccessionPlanRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanRequestCostRangeRule(double Min, double Max) : IGlobalSuccessionPlanRequestRule
{
    public List<string> Check(GlobalSuccessionPlanRequest item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalSuccessionPlanRequestManagerIdLengthRule(int MaxLength) : IGlobalSuccessionPlanRequestRule
{
    public List<string> Check(GlobalSuccessionPlanRequest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanRequestRequiredUnitRule(string Message) : IGlobalSuccessionPlanRequestRule
{
    public List<string> Check(GlobalSuccessionPlanRequest item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanRequestPriorityRangeRule(int Min, int Max) : IGlobalSuccessionPlanRequestRule
{
    public List<string> Check(GlobalSuccessionPlanRequest item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global succession plan request.</summary>
public sealed record GlobalSuccessionPlanRequestValidator(List<IGlobalSuccessionPlanRequestRule> Rules)
{
    public List<string> Validate(GlobalSuccessionPlanRequest item)
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

    public void RequireValid(GlobalSuccessionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalSuccessionPlanRequest> items)
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

/// <summary>Functions of global succession plan request rules.</summary>
public static class GlobalSuccessionPlanRequestRules
{
    public static GlobalSuccessionPlanRequestValidator DefaultGlobalSuccessionPlanRequestValidator()
    {
        return new GlobalSuccessionPlanRequestValidator(
            new List<IGlobalSuccessionPlanRequestRule>
            {
                new GlobalSuccessionPlanRequestLabelLengthRule(86400),
                new GlobalSuccessionPlanRequestCostRangeRule(2.6, 1052.0),
                new GlobalSuccessionPlanRequestManagerIdLengthRule(255),
                new GlobalSuccessionPlanRequestRequiredUnitRule("global succession plan request needs a value"),
                new GlobalSuccessionPlanRequestPriorityRangeRule(16, 86400),
            });
    }
}
