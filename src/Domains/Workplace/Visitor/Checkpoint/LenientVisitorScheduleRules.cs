namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient visitor schedule must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorScheduleRule
{
    List<string> Check(LenientVisitorSchedule item);
}

public sealed record LenientVisitorScheduleRequiredCodeRule(string Message) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorScheduleMaximumRangeRule(double Min, double Max) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorScheduleGraceDaysRangeRule(int Min, int Max) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientVisitorScheduleAllowedLabelRule(List<string> Allowed) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorScheduleRequiredLabelRule(string Message) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorScheduleCodeLengthRule(int MaxLength) : ILenientVisitorScheduleRule
{
    public List<string> Check(LenientVisitorSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor schedule.</summary>
public sealed record LenientVisitorScheduleValidator(List<ILenientVisitorScheduleRule> Rules)
{
    public List<string> Validate(LenientVisitorSchedule item)
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

    public void RequireValid(LenientVisitorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorSchedule> items)
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

/// <summary>Functions of lenient visitor schedule rules.</summary>
public static class LenientVisitorScheduleRules
{
    public static LenientVisitorScheduleValidator DefaultLenientVisitorScheduleValidator()
    {
        return new LenientVisitorScheduleValidator(
            new List<ILenientVisitorScheduleRule>
            {
                new LenientVisitorScheduleRequiredCodeRule("lenient visitor schedule needs this field"),
                new LenientVisitorScheduleMaximumRangeRule(1.5, 3505.7),
                new LenientVisitorScheduleGraceDaysRangeRule(32, 80),
                new LenientVisitorScheduleAllowedLabelRule(new List<string>()),
                new LenientVisitorScheduleRequiredLabelRule("lenient visitor schedule needs a value"),
                new LenientVisitorScheduleCodeLengthRule(120),
            });
    }
}
