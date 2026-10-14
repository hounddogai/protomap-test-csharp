namespace Acme.Workforce.Assets.Refresh.Tier;

/// <summary>A rule a refresh tier must pass. Check returns its problems, or none.</summary>
public interface IRefreshTierRule
{
    List<string> Check(RefreshTier item);
}

public sealed record RefreshTierRequiredCodeRule(string Message) : IRefreshTierRule
{
    public List<string> Check(RefreshTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTierHoursRangeRule(double Min, double Max) : IRefreshTierRule
{
    public List<string> Check(RefreshTier item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTierCodeLengthRule(int MaxLength) : IRefreshTierRule
{
    public List<string> Check(RefreshTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTierMinimumRangeRule(double Min, double Max) : IRefreshTierRule
{
    public List<string> Check(RefreshTier item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh tier.</summary>
public sealed record RefreshTierValidator(List<IRefreshTierRule> Rules)
{
    public List<string> Validate(RefreshTier item)
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

    public void RequireValid(RefreshTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshTier> items)
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

/// <summary>Functions of refresh tier rules.</summary>
public static class RefreshTierRules
{
    public static RefreshTierValidator DefaultRefreshTierValidator()
    {
        return new RefreshTierValidator(
            new List<IRefreshTierRule>
            {
                new RefreshTierRequiredCodeRule("refresh tier needs this field"),
                new RefreshTierHoursRangeRule(4.3, 2750.7),
                new RefreshTierCodeLengthRule(120),
                new RefreshTierMinimumRangeRule(1.0, 4653.9),
            });
    }
}
