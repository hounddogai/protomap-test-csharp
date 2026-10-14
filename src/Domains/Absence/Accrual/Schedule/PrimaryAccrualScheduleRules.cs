namespace Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>A rule a primary accrual schedule must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccrualScheduleRule
{
    List<string> Check(PrimaryAccrualSchedule item);
}

public sealed record PrimaryAccrualScheduleRequiredCodeRule(string Message) : IPrimaryAccrualScheduleRule
{
    public List<string> Check(PrimaryAccrualSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualScheduleRequiredTitleRule(string Message) : IPrimaryAccrualScheduleRule
{
    public List<string> Check(PrimaryAccrualSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualScheduleRatingRangeRule(double Min, double Max) : IPrimaryAccrualScheduleRule
{
    public List<string> Check(PrimaryAccrualSchedule item)
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

public sealed record PrimaryAccrualScheduleCodeLengthRule(int MaxLength) : IPrimaryAccrualScheduleRule
{
    public List<string> Check(PrimaryAccrualSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary accrual schedule.</summary>
public sealed record PrimaryAccrualScheduleValidator(List<IPrimaryAccrualScheduleRule> Rules)
{
    public List<string> Validate(PrimaryAccrualSchedule item)
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

    public void RequireValid(PrimaryAccrualSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccrualSchedule> items)
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

/// <summary>Functions of primary accrual schedule rules.</summary>
public static class PrimaryAccrualScheduleRules
{
    public static PrimaryAccrualScheduleValidator DefaultPrimaryAccrualScheduleValidator()
    {
        return new PrimaryAccrualScheduleValidator(
            new List<IPrimaryAccrualScheduleRule>
            {
                new PrimaryAccrualScheduleRequiredCodeRule("primary accrual schedule needs an entry"),
                new PrimaryAccrualScheduleRequiredTitleRule("primary accrual schedule needs an entry"),
                new PrimaryAccrualScheduleRatingRangeRule(5.4, 2739.8),
                new PrimaryAccrualScheduleCodeLengthRule(120),
            });
    }
}
