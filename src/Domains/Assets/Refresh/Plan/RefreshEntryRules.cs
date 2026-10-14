namespace Acme.Workforce.Assets.Refresh.Plan;

/// <summary>A rule a refresh entry must pass. Check returns its problems, or none.</summary>
public interface IRefreshEntryRule
{
    List<string> Check(RefreshEntry item);
}

public sealed record RefreshEntryGraceDaysRangeRule(int Min, int Max) : IRefreshEntryRule
{
    public List<string> Check(RefreshEntry item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshEntryRegionLengthRule(int MaxLength) : IRefreshEntryRule
{
    public List<string> Check(RefreshEntry item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshEntryLabelLengthRule(int MaxLength) : IRefreshEntryRule
{
    public List<string> Check(RefreshEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshEntryRequiredCodeRule(string Message) : IRefreshEntryRule
{
    public List<string> Check(RefreshEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh entry.</summary>
public sealed record RefreshEntryValidator(List<IRefreshEntryRule> Rules)
{
    public List<string> Validate(RefreshEntry item)
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

    public void RequireValid(RefreshEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshEntry> items)
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

/// <summary>Functions of refresh entry rules.</summary>
public static class RefreshEntryRules
{
    public static RefreshEntryValidator DefaultRefreshEntryValidator()
    {
        return new RefreshEntryValidator(
            new List<IRefreshEntryRule>
            {
                new RefreshEntryGraceDaysRangeRule(80, 86400),
                new RefreshEntryRegionLengthRule(32),
                new RefreshEntryLabelLengthRule(3600),
                new RefreshEntryRequiredCodeRule("refresh entry needs this field"),
            });
    }
}
