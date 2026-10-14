namespace Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>A rule a strict asset return segment must pass. Check returns its problems, or none.</summary>
public interface IStrictAssetReturnSegmentRule
{
    List<string> Check(StrictAssetReturnSegment item);
}

public sealed record StrictAssetReturnSegmentRequiredCodeRule(string Message) : IStrictAssetReturnSegmentRule
{
    public List<string> Check(StrictAssetReturnSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnSegmentCodeLengthRule(int MaxLength) : IStrictAssetReturnSegmentRule
{
    public List<string> Check(StrictAssetReturnSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnSegmentLeaveDaysRangeRule(int Min, int Max) : IStrictAssetReturnSegmentRule
{
    public List<string> Check(StrictAssetReturnSegment item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAssetReturnSegmentArchivedGuardRule(int Limit) : IStrictAssetReturnSegmentRule
{
    public List<string> Check(StrictAssetReturnSegment item)
    {
        if (item.Archived && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict asset return segment.</summary>
public sealed record StrictAssetReturnSegmentValidator(List<IStrictAssetReturnSegmentRule> Rules)
{
    public List<string> Validate(StrictAssetReturnSegment item)
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

    public void RequireValid(StrictAssetReturnSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssetReturnSegment> items)
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

/// <summary>Functions of strict asset return segment rules.</summary>
public static class StrictAssetReturnSegmentRules
{
    public static StrictAssetReturnSegmentValidator DefaultStrictAssetReturnSegmentValidator()
    {
        return new StrictAssetReturnSegmentValidator(
            new List<IStrictAssetReturnSegmentRule>
            {
                new StrictAssetReturnSegmentRequiredCodeRule("strict asset return segment needs a value"),
                new StrictAssetReturnSegmentCodeLengthRule(16),
                new StrictAssetReturnSegmentLeaveDaysRangeRule(32, 1000),
                new StrictAssetReturnSegmentArchivedGuardRule(86400),
            });
    }
}
