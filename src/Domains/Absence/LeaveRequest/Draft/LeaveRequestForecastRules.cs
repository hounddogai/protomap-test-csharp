namespace Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>A rule a leave request forecast must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestForecastRule
{
    List<string> Check(LeaveRequestForecast item);
}

public sealed record LeaveRequestForecastRequiredCodeRule(string Message) : ILeaveRequestForecastRule
{
    public List<string> Check(LeaveRequestForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestForecastRequiredTitleRule(string Message) : ILeaveRequestForecastRule
{
    public List<string> Check(LeaveRequestForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestForecastCodeLengthRule(int MaxLength) : ILeaveRequestForecastRule
{
    public List<string> Check(LeaveRequestForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestForecastGradeLengthRule(int MaxLength) : ILeaveRequestForecastRule
{
    public List<string> Check(LeaveRequestForecast item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request forecast.</summary>
public sealed record LeaveRequestForecastValidator(List<ILeaveRequestForecastRule> Rules)
{
    public List<string> Validate(LeaveRequestForecast item)
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

    public void RequireValid(LeaveRequestForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestForecast> items)
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

/// <summary>Functions of leave request forecast rules.</summary>
public static class LeaveRequestForecastRules
{
    public static LeaveRequestForecastValidator DefaultLeaveRequestForecastValidator()
    {
        return new LeaveRequestForecastValidator(
            new List<ILeaveRequestForecastRule>
            {
                new LeaveRequestForecastRequiredCodeRule("leave request forecast needs an entry"),
                new LeaveRequestForecastRequiredTitleRule("leave request forecast needs an entry"),
                new LeaveRequestForecastCodeLengthRule(3600),
                new LeaveRequestForecastGradeLengthRule(255),
            });
    }
}
