namespace Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>A rule a refresh score must pass. Check returns its problems, or none.</summary>
public interface IRefreshScoreRule
{
    List<string> Check(RefreshScore item);
}

public sealed record RefreshScoreRequiredCodeRule(string Message) : IRefreshScoreRule
{
    public List<string> Check(RefreshScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshScoreRequiredDepartmentRule(string Message) : IRefreshScoreRule
{
    public List<string> Check(RefreshScore item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshScoreCostRangeRule(double Min, double Max) : IRefreshScoreRule
{
    public List<string> Check(RefreshScore item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshScoreNameLengthRule(int MaxLength) : IRefreshScoreRule
{
    public List<string> Check(RefreshScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh score.</summary>
public sealed record RefreshScoreValidator(List<IRefreshScoreRule> Rules)
{
    public List<string> Validate(RefreshScore item)
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

    public void RequireValid(RefreshScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshScore> items)
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

/// <summary>Functions of refresh score rules.</summary>
public static class RefreshScoreRules
{
    public static RefreshScoreValidator DefaultRefreshScoreValidator()
    {
        return new RefreshScoreValidator(
            new List<IRefreshScoreRule>
            {
                new RefreshScoreRequiredCodeRule("refresh score needs an entry"),
                new RefreshScoreRequiredDepartmentRule("refresh score needs an entry"),
                new RefreshScoreCostRangeRule(5.8, 4600.2),
                new RefreshScoreNameLengthRule(80),
            });
    }
}
