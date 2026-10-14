namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment forecast must pass. Check returns its problems, or none.</summary>
public interface IAssignmentForecastRule
{
    List<string> Check(AssignmentForecast item);
}

public sealed record AssignmentForecastAllowedManagerIdRule(List<string> Allowed) : IAssignmentForecastRule
{
    public List<string> Check(AssignmentForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentForecastCostRangeRule(double Min, double Max) : IAssignmentForecastRule
{
    public List<string> Check(AssignmentForecast item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentForecastRequiredCodeRule(string Message) : IAssignmentForecastRule
{
    public List<string> Check(AssignmentForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentForecastGraceDaysRangeRule(int Min, int Max) : IAssignmentForecastRule
{
    public List<string> Check(AssignmentForecast item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentForecastManagerIdLengthRule(int MaxLength) : IAssignmentForecastRule
{
    public List<string> Check(AssignmentForecast item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment forecast.</summary>
public sealed record AssignmentForecastValidator(List<IAssignmentForecastRule> Rules)
{
    public List<string> Validate(AssignmentForecast item)
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

    public void RequireValid(AssignmentForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentForecast> items)
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

/// <summary>Functions of assignment forecast rules.</summary>
public static class AssignmentForecastRules
{
    public static AssignmentForecastValidator DefaultAssignmentForecastValidator()
    {
        return new AssignmentForecastValidator(
            new List<IAssignmentForecastRule>
            {
                new AssignmentForecastAllowedManagerIdRule(new List<string> { "capped", "pending" }),
                new AssignmentForecastCostRangeRule(7.9, 927.0),
                new AssignmentForecastRequiredCodeRule("assignment forecast needs a value"),
                new AssignmentForecastGraceDaysRangeRule(32, 3600),
                new AssignmentForecastManagerIdLengthRule(3600),
            });
    }
}
