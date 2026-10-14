namespace Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>A rule a refresh override must pass. Check returns its problems, or none.</summary>
public interface IRefreshOverrideRule
{
    List<string> Check(RefreshOverride item);
}

public sealed record RefreshOverrideMarginRangeRule(double Min, double Max) : IRefreshOverrideRule
{
    public List<string> Check(RefreshOverride item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshOverridePinnedGuardRule(int Limit) : IRefreshOverrideRule
{
    public List<string> Check(RefreshOverride item)
    {
        if (item.Pinned && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RefreshOverrideRequiredLabelRule(string Message) : IRefreshOverrideRule
{
    public List<string> Check(RefreshOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshOverrideCodeLengthRule(int MaxLength) : IRefreshOverrideRule
{
    public List<string> Check(RefreshOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshOverrideRateRangeRule(double Min, double Max) : IRefreshOverrideRule
{
    public List<string> Check(RefreshOverride item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh override.</summary>
public sealed record RefreshOverrideValidator(List<IRefreshOverrideRule> Rules)
{
    public List<string> Validate(RefreshOverride item)
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

    public void RequireValid(RefreshOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshOverride> items)
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

/// <summary>Functions of refresh override rules.</summary>
public static class RefreshOverrideRules
{
    public static RefreshOverrideValidator DefaultRefreshOverrideValidator()
    {
        return new RefreshOverrideValidator(
            new List<IRefreshOverrideRule>
            {
                new RefreshOverrideMarginRangeRule(1.8, 4702.7),
                new RefreshOverridePinnedGuardRule(64),
                new RefreshOverrideRequiredLabelRule("refresh override needs an entry"),
                new RefreshOverrideCodeLengthRule(80),
                new RefreshOverrideRateRangeRule(6.5, 935.0),
            });
    }
}
