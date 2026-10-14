namespace Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>A rule a fallback bonus pool schedule must pass. Check returns its problems, or none.</summary>
public interface IFallbackBonusPoolScheduleRule
{
    List<string> Check(FallbackBonusPoolSchedule item);
}

public sealed record FallbackBonusPoolScheduleBadgeLengthRule(int MaxLength) : IFallbackBonusPoolScheduleRule
{
    public List<string> Check(FallbackBonusPoolSchedule item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolScheduleRequiredNameRule(string Message) : IFallbackBonusPoolScheduleRule
{
    public List<string> Check(FallbackBonusPoolSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolScheduleRequiredBadgeRule(string Message) : IFallbackBonusPoolScheduleRule
{
    public List<string> Check(FallbackBonusPoolSchedule item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback bonus pool schedule.</summary>
public sealed record FallbackBonusPoolScheduleValidator(List<IFallbackBonusPoolScheduleRule> Rules)
{
    public List<string> Validate(FallbackBonusPoolSchedule item)
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

    public void RequireValid(FallbackBonusPoolSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBonusPoolSchedule> items)
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

/// <summary>Functions of fallback bonus pool schedule rules.</summary>
public static class FallbackBonusPoolScheduleRules
{
    public static FallbackBonusPoolScheduleValidator DefaultFallbackBonusPoolScheduleValidator()
    {
        return new FallbackBonusPoolScheduleValidator(
            new List<IFallbackBonusPoolScheduleRule>
            {
                new FallbackBonusPoolScheduleBadgeLengthRule(80),
                new FallbackBonusPoolScheduleRequiredNameRule("fallback bonus pool schedule needs a value"),
                new FallbackBonusPoolScheduleRequiredBadgeRule("fallback bonus pool schedule needs this field"),
            });
    }
}
