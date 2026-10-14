namespace Acme.Workforce.Assets.Refresh.Policy;

/// <summary>A rule a refresh estimate must pass. Check returns its problems, or none.</summary>
public interface IRefreshEstimateRule
{
    List<string> Check(RefreshEstimate item);
}

public sealed record RefreshEstimateDescriptionLengthRule(int MaxLength) : IRefreshEstimateRule
{
    public List<string> Check(RefreshEstimate item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshEstimateRetriesRangeRule(int Min, int Max) : IRefreshEstimateRule
{
    public List<string> Check(RefreshEstimate item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshEstimateVersionRangeRule(int Min, int Max) : IRefreshEstimateRule
{
    public List<string> Check(RefreshEstimate item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshEstimateRequiredCodeRule(string Message) : IRefreshEstimateRule
{
    public List<string> Check(RefreshEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshEstimateRequiredManagerIdRule(string Message) : IRefreshEstimateRule
{
    public List<string> Check(RefreshEstimate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh estimate.</summary>
public sealed record RefreshEstimateValidator(List<IRefreshEstimateRule> Rules)
{
    public List<string> Validate(RefreshEstimate item)
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

    public void RequireValid(RefreshEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshEstimate> items)
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

/// <summary>Functions of refresh estimate rules.</summary>
public static class RefreshEstimateRules
{
    public static RefreshEstimateValidator DefaultRefreshEstimateValidator()
    {
        return new RefreshEstimateValidator(
            new List<IRefreshEstimateRule>
            {
                new RefreshEstimateDescriptionLengthRule(1000),
                new RefreshEstimateRetriesRangeRule(16, 16),
                new RefreshEstimateVersionRangeRule(16, 3600),
                new RefreshEstimateRequiredCodeRule("refresh estimate needs an entry"),
                new RefreshEstimateRequiredManagerIdRule("refresh estimate needs this field"),
            });
    }
}
