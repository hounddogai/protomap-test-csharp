namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request review must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestReviewRule
{
    List<string> Check(LeaveRequestReview item);
}

public sealed record LeaveRequestReviewArchivedGuardRule(double Limit) : ILeaveRequestReviewRule
{
    public List<string> Check(LeaveRequestReview item)
    {
        if (item.Archived && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestReviewRequiredDepartmentRule(string Message) : ILeaveRequestReviewRule
{
    public List<string> Check(LeaveRequestReview item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestReviewAllowedLabelRule(List<string> Allowed) : ILeaveRequestReviewRule
{
    public List<string> Check(LeaveRequestReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestReviewDepartmentLengthRule(int MaxLength) : ILeaveRequestReviewRule
{
    public List<string> Check(LeaveRequestReview item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestReviewRatingRangeRule(double Min, double Max) : ILeaveRequestReviewRule
{
    public List<string> Check(LeaveRequestReview item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a leave request review.</summary>
public sealed record LeaveRequestReviewValidator(List<ILeaveRequestReviewRule> Rules)
{
    public List<string> Validate(LeaveRequestReview item)
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

    public void RequireValid(LeaveRequestReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestReview> items)
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

/// <summary>Functions of leave request review rules.</summary>
public static class LeaveRequestReviewRules
{
    public static LeaveRequestReviewValidator DefaultLeaveRequestReviewValidator()
    {
        return new LeaveRequestReviewValidator(
            new List<ILeaveRequestReviewRule>
            {
                new LeaveRequestReviewArchivedGuardRule(8.8),
                new LeaveRequestReviewRequiredDepartmentRule("leave request review needs a value"),
                new LeaveRequestReviewAllowedLabelRule(new List<string> { "seasonal", "capped" }),
                new LeaveRequestReviewDepartmentLengthRule(16),
                new LeaveRequestReviewRatingRangeRule(3.7, 3171.9),
            });
    }
}
