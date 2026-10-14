namespace Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>A rule a capped career path schedule must pass. Check returns its problems, or none.</summary>
public interface ICappedCareerPathScheduleRule
{
    List<string> Check(CappedCareerPathSchedule item);
}

public sealed record CappedCareerPathScheduleSequenceRangeRule(int Min, int Max) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedCareerPathScheduleCodeLengthRule(int MaxLength) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathScheduleWindowDaysRangeRule(int Min, int Max) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedCareerPathScheduleRequiredCurrencyRule(string Message) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathScheduleCurrencyLengthRule(int MaxLength) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathScheduleLocaleLengthRule(int MaxLength) : ICappedCareerPathScheduleRule
{
    public List<string> Check(CappedCareerPathSchedule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped career path schedule.</summary>
public sealed record CappedCareerPathScheduleValidator(List<ICappedCareerPathScheduleRule> Rules)
{
    public List<string> Validate(CappedCareerPathSchedule item)
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

    public void RequireValid(CappedCareerPathSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCareerPathSchedule> items)
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

/// <summary>Functions of capped career path schedule rules.</summary>
public static class CappedCareerPathScheduleRules
{
    public static CappedCareerPathScheduleValidator DefaultCappedCareerPathScheduleValidator()
    {
        return new CappedCareerPathScheduleValidator(
            new List<ICappedCareerPathScheduleRule>
            {
                new CappedCareerPathScheduleSequenceRangeRule(255, 86400),
                new CappedCareerPathScheduleCodeLengthRule(86400),
                new CappedCareerPathScheduleWindowDaysRangeRule(120, 86400),
                new CappedCareerPathScheduleRequiredCurrencyRule("capped career path schedule needs an entry"),
                new CappedCareerPathScheduleCurrencyLengthRule(120),
                new CappedCareerPathScheduleLocaleLengthRule(255),
            });
    }
}
