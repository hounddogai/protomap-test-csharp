namespace Acme.Workforce.Assets.Refresh.Preference;

/// <summary>A rule a refresh assignment must pass. Check returns its problems, or none.</summary>
public interface IRefreshAssignmentRule
{
    List<string> Check(RefreshAssignment item);
}

public sealed record RefreshAssignmentFteRangeRule(double Min, double Max) : IRefreshAssignmentRule
{
    public List<string> Check(RefreshAssignment item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAssignmentBadgeLengthRule(int MaxLength) : IRefreshAssignmentRule
{
    public List<string> Check(RefreshAssignment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshAssignmentBatchSizeRangeRule(int Min, int Max) : IRefreshAssignmentRule
{
    public List<string> Check(RefreshAssignment item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshAssignmentRequiredCodeRule(string Message) : IRefreshAssignmentRule
{
    public List<string> Check(RefreshAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshAssignmentCodeLengthRule(int MaxLength) : IRefreshAssignmentRule
{
    public List<string> Check(RefreshAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh assignment.</summary>
public sealed record RefreshAssignmentValidator(List<IRefreshAssignmentRule> Rules)
{
    public List<string> Validate(RefreshAssignment item)
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

    public void RequireValid(RefreshAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshAssignment> items)
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

/// <summary>Functions of refresh assignment rules.</summary>
public static class RefreshAssignmentRules
{
    public static RefreshAssignmentValidator DefaultRefreshAssignmentValidator()
    {
        return new RefreshAssignmentValidator(
            new List<IRefreshAssignmentRule>
            {
                new RefreshAssignmentFteRangeRule(1.6, 3872.9),
                new RefreshAssignmentBadgeLengthRule(64),
                new RefreshAssignmentBatchSizeRangeRule(80, 86400),
                new RefreshAssignmentRequiredCodeRule("refresh assignment needs an entry"),
                new RefreshAssignmentCodeLengthRule(1000),
            });
    }
}
