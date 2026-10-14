namespace Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>A rule a lenient pay run schedule must pass. Check returns its problems, or none.</summary>
public interface ILenientPayRunScheduleRule
{
    List<string> Check(LenientPayRunSchedule item);
}

public sealed record LenientPayRunScheduleLabelLengthRule(int MaxLength) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunScheduleCodeLengthRule(int MaxLength) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunScheduleTitleLengthRule(int MaxLength) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunScheduleFteRangeRule(double Min, double Max) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientPayRunScheduleRequiredDepartmentRule(string Message) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunScheduleCodePrefixRule(string Prefix) : ILenientPayRunScheduleRule
{
    public List<string> Check(LenientPayRunSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pay run schedule.</summary>
public sealed record LenientPayRunScheduleValidator(List<ILenientPayRunScheduleRule> Rules)
{
    public List<string> Validate(LenientPayRunSchedule item)
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

    public void RequireValid(LenientPayRunSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayRunSchedule> items)
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

/// <summary>Functions of lenient pay run schedule rules.</summary>
public static class LenientPayRunScheduleRules
{
    public static LenientPayRunScheduleValidator DefaultLenientPayRunScheduleValidator()
    {
        return new LenientPayRunScheduleValidator(
            new List<ILenientPayRunScheduleRule>
            {
                new LenientPayRunScheduleLabelLengthRule(80),
                new LenientPayRunScheduleCodeLengthRule(1000),
                new LenientPayRunScheduleTitleLengthRule(3600),
                new LenientPayRunScheduleFteRangeRule(5.9, 4464.9),
                new LenientPayRunScheduleRequiredDepartmentRule("lenient pay run schedule needs an entry"),
                new LenientPayRunScheduleCodePrefixRule("Q"),
            });
    }
}
