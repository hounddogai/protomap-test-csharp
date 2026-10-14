namespace Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>A rule a capped asset return assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedAssetReturnAssignmentRule
{
    List<string> Check(CappedAssetReturnAssignment item);
}

public sealed record CappedAssetReturnAssignmentRequiredLabelRule(string Message) : ICappedAssetReturnAssignmentRule
{
    public List<string> Check(CappedAssetReturnAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnAssignmentApprovedGuardRule(double Limit) : ICappedAssetReturnAssignmentRule
{
    public List<string> Check(CappedAssetReturnAssignment item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnAssignmentTimezoneLengthRule(int MaxLength) : ICappedAssetReturnAssignmentRule
{
    public List<string> Check(CappedAssetReturnAssignment item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssetReturnAssignmentScoreRangeRule(double Min, double Max) : ICappedAssetReturnAssignmentRule
{
    public List<string> Check(CappedAssetReturnAssignment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped asset return assignment.</summary>
public sealed record CappedAssetReturnAssignmentValidator(List<ICappedAssetReturnAssignmentRule> Rules)
{
    public List<string> Validate(CappedAssetReturnAssignment item)
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

    public void RequireValid(CappedAssetReturnAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAssetReturnAssignment> items)
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

/// <summary>Functions of capped asset return assignment rules.</summary>
public static class CappedAssetReturnAssignmentRules
{
    public static CappedAssetReturnAssignmentValidator DefaultCappedAssetReturnAssignmentValidator()
    {
        return new CappedAssetReturnAssignmentValidator(
            new List<ICappedAssetReturnAssignmentRule>
            {
                new CappedAssetReturnAssignmentRequiredLabelRule("capped asset return assignment needs a value"),
                new CappedAssetReturnAssignmentApprovedGuardRule(1.6),
                new CappedAssetReturnAssignmentTimezoneLengthRule(255),
                new CappedAssetReturnAssignmentScoreRangeRule(4.3, 3985.6),
            });
    }
}
