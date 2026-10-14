namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request segment must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestSegmentRule
{
    List<string> Check(LeaveRequestSegment item);
}

public sealed record LeaveRequestSegmentScoreRangeRule(double Min, double Max) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
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

public sealed record LeaveRequestSegmentCostRangeRule(double Min, double Max) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestSegmentRequiredSkillRule(string Message) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSegmentAllowedTeamRule(List<string> Allowed) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSegmentSkillLengthRule(int MaxLength) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSegmentRequiredSalaryBandRule(string Message) : ILeaveRequestSegmentRule
{
    public List<string> Check(LeaveRequestSegment item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request segment.</summary>
public sealed record LeaveRequestSegmentValidator(List<ILeaveRequestSegmentRule> Rules)
{
    public List<string> Validate(LeaveRequestSegment item)
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

    public void RequireValid(LeaveRequestSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestSegment> items)
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

/// <summary>Functions of leave request segment rules.</summary>
public static class LeaveRequestSegmentRules
{
    public static LeaveRequestSegmentValidator DefaultLeaveRequestSegmentValidator()
    {
        return new LeaveRequestSegmentValidator(
            new List<ILeaveRequestSegmentRule>
            {
                new LeaveRequestSegmentScoreRangeRule(6.5, 3647.2),
                new LeaveRequestSegmentCostRangeRule(5.0, 2248.3),
                new LeaveRequestSegmentRequiredSkillRule("leave request segment needs a value"),
                new LeaveRequestSegmentAllowedTeamRule(new List<string>()),
                new LeaveRequestSegmentSkillLengthRule(86400),
                new LeaveRequestSegmentRequiredSalaryBandRule("leave request segment needs a value"),
            });
    }
}
