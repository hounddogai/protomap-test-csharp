namespace Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>A rule a asset return schedule must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnScheduleRule
{
    List<string> Check(AssetReturnSchedule item);
}

public sealed record AssetReturnScheduleRevisionRangeRule(int Min, int Max) : IAssetReturnScheduleRule
{
    public List<string> Check(AssetReturnSchedule item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScheduleTitleLengthRule(int MaxLength) : IAssetReturnScheduleRule
{
    public List<string> Check(AssetReturnSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScheduleRequiredCodeRule(string Message) : IAssetReturnScheduleRule
{
    public List<string> Check(AssetReturnSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnScheduleTenureMonthsRangeRule(int Min, int Max) : IAssetReturnScheduleRule
{
    public List<string> Check(AssetReturnSchedule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a asset return schedule.</summary>
public sealed record AssetReturnScheduleValidator(List<IAssetReturnScheduleRule> Rules)
{
    public List<string> Validate(AssetReturnSchedule item)
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

    public void RequireValid(AssetReturnSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnSchedule> items)
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

/// <summary>Functions of asset return schedule rules.</summary>
public static class AssetReturnScheduleRules
{
    public static AssetReturnScheduleValidator DefaultAssetReturnScheduleValidator()
    {
        return new AssetReturnScheduleValidator(
            new List<IAssetReturnScheduleRule>
            {
                new AssetReturnScheduleRevisionRangeRule(3600, 86400),
                new AssetReturnScheduleTitleLengthRule(86400),
                new AssetReturnScheduleRequiredCodeRule("asset return schedule needs this field"),
                new AssetReturnScheduleTenureMonthsRangeRule(16, 32),
            });
    }
}
