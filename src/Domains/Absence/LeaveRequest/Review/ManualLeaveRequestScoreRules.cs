namespace Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>A rule a manual leave request score must pass. Check returns its problems, or none.</summary>
public interface IManualLeaveRequestScoreRule
{
    List<string> Check(ManualLeaveRequestScore item);
}

public sealed record ManualLeaveRequestScoreRequiredNameRule(string Message) : IManualLeaveRequestScoreRule
{
    public List<string> Check(ManualLeaveRequestScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestScoreJobCodeLengthRule(int MaxLength) : IManualLeaveRequestScoreRule
{
    public List<string> Check(ManualLeaveRequestScore item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestScoreRequiredCategoryRule(string Message) : IManualLeaveRequestScoreRule
{
    public List<string> Check(ManualLeaveRequestScore item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual leave request score.</summary>
public sealed record ManualLeaveRequestScoreValidator(List<IManualLeaveRequestScoreRule> Rules)
{
    public List<string> Validate(ManualLeaveRequestScore item)
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

    public void RequireValid(ManualLeaveRequestScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLeaveRequestScore> items)
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

/// <summary>Functions of manual leave request score rules.</summary>
public static class ManualLeaveRequestScoreRules
{
    public static ManualLeaveRequestScoreValidator DefaultManualLeaveRequestScoreValidator()
    {
        return new ManualLeaveRequestScoreValidator(
            new List<IManualLeaveRequestScoreRule>
            {
                new ManualLeaveRequestScoreRequiredNameRule("manual leave request score needs a value"),
                new ManualLeaveRequestScoreJobCodeLengthRule(16),
                new ManualLeaveRequestScoreRequiredCategoryRule("manual leave request score needs a value"),
            });
    }
}
