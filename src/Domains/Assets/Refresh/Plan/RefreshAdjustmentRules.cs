namespace Acme.Workforce.Assets.Refresh.Plan;

/// <summary>A rule a refresh adjustment must pass. Check returns its problems, or none.</summary>
public interface IRefreshAdjustmentRule
{
    List<string> Check(RefreshAdjustment item);
}

public sealed record RefreshAdjustmentMarginRangeRule(double Min, double Max) : IRefreshAdjustmentRule
{
    public List<string> Check(RefreshAdjustment item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAdjustmentSourceLengthRule(int MaxLength) : IRefreshAdjustmentRule
{
    public List<string> Check(RefreshAdjustment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAdjustmentRequiredDescriptionRule(string Message) : IRefreshAdjustmentRule
{
    public List<string> Check(RefreshAdjustment item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh adjustment.</summary>
public sealed record RefreshAdjustmentValidator(List<IRefreshAdjustmentRule> Rules)
{
    public List<string> Validate(RefreshAdjustment item)
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

    public void RequireValid(RefreshAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshAdjustment> items)
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

/// <summary>Functions of refresh adjustment rules.</summary>
public static class RefreshAdjustmentRules
{
    public static RefreshAdjustmentValidator DefaultRefreshAdjustmentValidator()
    {
        return new RefreshAdjustmentValidator(
            new List<IRefreshAdjustmentRule>
            {
                new RefreshAdjustmentMarginRangeRule(0.3, 653.1),
                new RefreshAdjustmentSourceLengthRule(16),
                new RefreshAdjustmentRequiredDescriptionRule("refresh adjustment needs this field"),
            });
    }
}
