namespace Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>A rule a fallback holiday schedule must pass. Check returns its problems, or none.</summary>
public interface IFallbackHolidayScheduleRule
{
    List<string> Check(FallbackHolidaySchedule item);
}

public sealed record FallbackHolidayScheduleCodePrefixRule(string Prefix) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidayScheduleTitleLengthRule(int MaxLength) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidayScheduleCodeLengthRule(int MaxLength) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidayScheduleRequiredTitleRule(string Message) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidayScheduleRequiredExternalRefRule(string Message) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidayScheduleTimezoneLengthRule(int MaxLength) : IFallbackHolidayScheduleRule
{
    public List<string> Check(FallbackHolidaySchedule item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback holiday schedule.</summary>
public sealed record FallbackHolidayScheduleValidator(List<IFallbackHolidayScheduleRule> Rules)
{
    public List<string> Validate(FallbackHolidaySchedule item)
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

    public void RequireValid(FallbackHolidaySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackHolidaySchedule> items)
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

/// <summary>Functions of fallback holiday schedule rules.</summary>
public static class FallbackHolidayScheduleRules
{
    public static FallbackHolidayScheduleValidator DefaultFallbackHolidayScheduleValidator()
    {
        return new FallbackHolidayScheduleValidator(
            new List<IFallbackHolidayScheduleRule>
            {
                new FallbackHolidayScheduleCodePrefixRule("Q"),
                new FallbackHolidayScheduleTitleLengthRule(32),
                new FallbackHolidayScheduleCodeLengthRule(64),
                new FallbackHolidayScheduleRequiredTitleRule("fallback holiday schedule needs this field"),
                new FallbackHolidayScheduleRequiredExternalRefRule("fallback holiday schedule needs this field"),
                new FallbackHolidayScheduleTimezoneLengthRule(32),
            });
    }
}
