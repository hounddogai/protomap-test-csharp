namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>A rule a regional leave request baseline must pass. Check returns its problems, or none.</summary>
public interface IRegionalLeaveRequestBaselineRule
{
    List<string> Check(RegionalLeaveRequestBaseline item);
}

public sealed record RegionalLeaveRequestBaselineRequiredSegmentRule(string Message) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestBaselineCodeLengthRule(int MaxLength) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestBaselinePinnedGuardRule(int Limit) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
    {
        if (item.Pinned && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestBaselineRequiredSummaryRule(string Message) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestBaselineRequiredCodeRule(string Message) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestBaselineScoreRangeRule(double Min, double Max) : IRegionalLeaveRequestBaselineRule
{
    public List<string> Check(RegionalLeaveRequestBaseline item)
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

/// <summary>Runs every rule of a regional leave request baseline.</summary>
public sealed record RegionalLeaveRequestBaselineValidator(List<IRegionalLeaveRequestBaselineRule> Rules)
{
    public List<string> Validate(RegionalLeaveRequestBaseline item)
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

    public void RequireValid(RegionalLeaveRequestBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalLeaveRequestBaseline> items)
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

/// <summary>Functions of regional leave request baseline rules.</summary>
public static class RegionalLeaveRequestBaselineRules
{
    public static RegionalLeaveRequestBaselineValidator DefaultRegionalLeaveRequestBaselineValidator()
    {
        return new RegionalLeaveRequestBaselineValidator(
            new List<IRegionalLeaveRequestBaselineRule>
            {
                new RegionalLeaveRequestBaselineRequiredSegmentRule("regional leave request baseline needs a value"),
                new RegionalLeaveRequestBaselineCodeLengthRule(3600),
                new RegionalLeaveRequestBaselinePinnedGuardRule(16),
                new RegionalLeaveRequestBaselineRequiredSummaryRule("regional leave request baseline needs a value"),
                new RegionalLeaveRequestBaselineRequiredCodeRule("regional leave request baseline needs an entry"),
                new RegionalLeaveRequestBaselineScoreRangeRule(4.4, 2695.2),
            });
    }
}
