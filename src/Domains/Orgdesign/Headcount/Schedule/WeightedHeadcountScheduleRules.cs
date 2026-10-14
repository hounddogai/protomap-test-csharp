namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>A rule a weighted headcount schedule must pass. Check returns its problems, or none.</summary>
public interface IWeightedHeadcountScheduleRule
{
    List<string> Check(WeightedHeadcountSchedule item);
}

public sealed record WeightedHeadcountScheduleRequiredNameRule(string Message) : IWeightedHeadcountScheduleRule
{
    public List<string> Check(WeightedHeadcountSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountScheduleRequiredTeamRule(string Message) : IWeightedHeadcountScheduleRule
{
    public List<string> Check(WeightedHeadcountSchedule item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHeadcountScheduleCodeLengthRule(int MaxLength) : IWeightedHeadcountScheduleRule
{
    public List<string> Check(WeightedHeadcountSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted headcount schedule.</summary>
public sealed record WeightedHeadcountScheduleValidator(List<IWeightedHeadcountScheduleRule> Rules)
{
    public List<string> Validate(WeightedHeadcountSchedule item)
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

    public void RequireValid(WeightedHeadcountSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedHeadcountSchedule> items)
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

/// <summary>Functions of weighted headcount schedule rules.</summary>
public static class WeightedHeadcountScheduleRules
{
    public static WeightedHeadcountScheduleValidator DefaultWeightedHeadcountScheduleValidator()
    {
        return new WeightedHeadcountScheduleValidator(
            new List<IWeightedHeadcountScheduleRule>
            {
                new WeightedHeadcountScheduleRequiredNameRule("weighted headcount schedule needs an entry"),
                new WeightedHeadcountScheduleRequiredTeamRule("weighted headcount schedule needs an entry"),
                new WeightedHeadcountScheduleCodeLengthRule(80),
            });
    }
}
