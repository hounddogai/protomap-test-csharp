namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

/// <summary>A rule a return plan rollup must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanRollupRule
{
    List<string> Check(ReturnPlanRollup item);
}

public sealed record ReturnPlanRollupRequiredUnitRule(string Message) : IReturnPlanRollupRule
{
    public List<string> Check(ReturnPlanRollup item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRollupRequiredTeamRule(string Message) : IReturnPlanRollupRule
{
    public List<string> Check(ReturnPlanRollup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRollupTitleLengthRule(int MaxLength) : IReturnPlanRollupRule
{
    public List<string> Check(ReturnPlanRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan rollup.</summary>
public sealed record ReturnPlanRollupValidator(List<IReturnPlanRollupRule> Rules)
{
    public List<string> Validate(ReturnPlanRollup item)
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

    public void RequireValid(ReturnPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanRollup> items)
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

/// <summary>Functions of return plan rollup rules.</summary>
public static class ReturnPlanRollupRules
{
    public static ReturnPlanRollupValidator DefaultReturnPlanRollupValidator()
    {
        return new ReturnPlanRollupValidator(
            new List<IReturnPlanRollupRule>
            {
                new ReturnPlanRollupRequiredUnitRule("return plan rollup needs an entry"),
                new ReturnPlanRollupRequiredTeamRule("return plan rollup needs a value"),
                new ReturnPlanRollupTitleLengthRule(120),
            });
    }
}
