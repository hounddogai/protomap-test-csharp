namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback asset return segment must pass. Check returns its problems, or none.</summary>
public interface IFallbackAssetReturnSegmentRule
{
    List<string> Check(FallbackAssetReturnSegment item);
}

public sealed record FallbackAssetReturnSegmentRequiredTitleRule(string Message) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnSegmentRetriesRangeRule(int Min, int Max) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnSegmentRequiredCodeRule(string Message) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnSegmentCodeLengthRule(int MaxLength) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnSegmentEmployeeIdLengthRule(int MaxLength) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnSegmentAllowedEmployeeIdRule(List<string> Allowed) : IFallbackAssetReturnSegmentRule
{
    public List<string> Check(FallbackAssetReturnSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback asset return segment.</summary>
public sealed record FallbackAssetReturnSegmentValidator(List<IFallbackAssetReturnSegmentRule> Rules)
{
    public List<string> Validate(FallbackAssetReturnSegment item)
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

    public void RequireValid(FallbackAssetReturnSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAssetReturnSegment> items)
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

/// <summary>Functions of fallback asset return segment rules.</summary>
public static class FallbackAssetReturnSegmentRules
{
    public static FallbackAssetReturnSegmentValidator DefaultFallbackAssetReturnSegmentValidator()
    {
        return new FallbackAssetReturnSegmentValidator(
            new List<IFallbackAssetReturnSegmentRule>
            {
                new FallbackAssetReturnSegmentRequiredTitleRule("fallback asset return segment needs a value"),
                new FallbackAssetReturnSegmentRetriesRangeRule(16, 64),
                new FallbackAssetReturnSegmentRequiredCodeRule("fallback asset return segment needs a value"),
                new FallbackAssetReturnSegmentCodeLengthRule(1000),
                new FallbackAssetReturnSegmentEmployeeIdLengthRule(16),
                new FallbackAssetReturnSegmentAllowedEmployeeIdRule(
                    new List<string> { "scheduled", "strict", "fallback" }),
            });
    }
}
