namespace Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>A rule a bonus pool schedule must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolScheduleRule
{
    List<string> Check(BonusPoolSchedule item);
}

public sealed record BonusPoolScheduleLocaleLengthRule(int MaxLength) : IBonusPoolScheduleRule
{
    public List<string> Check(BonusPoolSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolScheduleRequiredCodeRule(string Message) : IBonusPoolScheduleRule
{
    public List<string> Check(BonusPoolSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolScheduleRequiredLocaleRule(string Message) : IBonusPoolScheduleRule
{
    public List<string> Check(BonusPoolSchedule item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool schedule.</summary>
public sealed record BonusPoolScheduleValidator(List<IBonusPoolScheduleRule> Rules)
{
    public List<string> Validate(BonusPoolSchedule item)
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

    public void RequireValid(BonusPoolSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolSchedule> items)
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

/// <summary>Functions of bonus pool schedule rules.</summary>
public static class BonusPoolScheduleRules
{
    public static BonusPoolScheduleValidator DefaultBonusPoolScheduleValidator()
    {
        return new BonusPoolScheduleValidator(
            new List<IBonusPoolScheduleRule>
            {
                new BonusPoolScheduleLocaleLengthRule(80),
                new BonusPoolScheduleRequiredCodeRule("bonus pool schedule needs this field"),
                new BonusPoolScheduleRequiredLocaleRule("bonus pool schedule needs this field"),
            });
    }
}
