namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>A rule a capped offboarding schedule must pass. Check returns its problems, or none.</summary>
public interface ICappedOffboardingScheduleRule
{
    List<string> Check(CappedOffboardingSchedule item);
}

public sealed record CappedOffboardingScheduleFloorValueRangeRule(double Min, double Max) : ICappedOffboardingScheduleRule
{
    public List<string> Check(CappedOffboardingSchedule item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedOffboardingScheduleLocaleLengthRule(int MaxLength) : ICappedOffboardingScheduleRule
{
    public List<string> Check(CappedOffboardingSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingScheduleVisibleGuardRule(int Limit) : ICappedOffboardingScheduleRule
{
    public List<string> Check(CappedOffboardingSchedule item)
    {
        if (item.Visible && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped offboarding schedule.</summary>
public sealed record CappedOffboardingScheduleValidator(List<ICappedOffboardingScheduleRule> Rules)
{
    public List<string> Validate(CappedOffboardingSchedule item)
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

    public void RequireValid(CappedOffboardingSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOffboardingSchedule> items)
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

/// <summary>Functions of capped offboarding schedule rules.</summary>
public static class CappedOffboardingScheduleRules
{
    public static CappedOffboardingScheduleValidator DefaultCappedOffboardingScheduleValidator()
    {
        return new CappedOffboardingScheduleValidator(
            new List<ICappedOffboardingScheduleRule>
            {
                new CappedOffboardingScheduleFloorValueRangeRule(3.8, 4207.0),
                new CappedOffboardingScheduleLocaleLengthRule(32),
                new CappedOffboardingScheduleVisibleGuardRule(1000),
            });
    }
}
