namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>A rule a succession plan exception must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanExceptionRule
{
    List<string> Check(SuccessionPlanException item);
}

public sealed record SuccessionPlanExceptionTitleLengthRule(int MaxLength) : ISuccessionPlanExceptionRule
{
    public List<string> Check(SuccessionPlanException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanExceptionLeaveDaysRangeRule(int Min, int Max) : ISuccessionPlanExceptionRule
{
    public List<string> Check(SuccessionPlanException item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanExceptionRequiredCodeRule(string Message) : ISuccessionPlanExceptionRule
{
    public List<string> Check(SuccessionPlanException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan exception.</summary>
public sealed record SuccessionPlanExceptionValidator(List<ISuccessionPlanExceptionRule> Rules)
{
    public List<string> Validate(SuccessionPlanException item)
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

    public void RequireValid(SuccessionPlanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanException> items)
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

/// <summary>Functions of succession plan exception rules.</summary>
public static class SuccessionPlanExceptionRules
{
    public static SuccessionPlanExceptionValidator DefaultSuccessionPlanExceptionValidator()
    {
        return new SuccessionPlanExceptionValidator(
            new List<ISuccessionPlanExceptionRule>
            {
                new SuccessionPlanExceptionTitleLengthRule(1000),
                new SuccessionPlanExceptionLeaveDaysRangeRule(32, 255),
                new SuccessionPlanExceptionRequiredCodeRule("succession plan exception needs an entry"),
            });
    }
}
