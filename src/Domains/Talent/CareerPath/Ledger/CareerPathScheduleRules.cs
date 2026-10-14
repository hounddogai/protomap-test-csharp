namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path schedule must pass. Check returns its problems, or none.</summary>
public interface ICareerPathScheduleRule
{
    List<string> Check(CareerPathSchedule item);
}

public sealed record CareerPathScheduleAllowedSegmentRule(List<string> Allowed) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathScheduleSegmentLengthRule(int MaxLength) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathScheduleRequiredContactEmailRule(string Message) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathScheduleRevisionRangeRule(int Min, int Max) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathScheduleRequiredSegmentRule(string Message) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathScheduleRequiredCodeRule(string Message) : ICareerPathScheduleRule
{
    public List<string> Check(CareerPathSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path schedule.</summary>
public sealed record CareerPathScheduleValidator(List<ICareerPathScheduleRule> Rules)
{
    public List<string> Validate(CareerPathSchedule item)
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

    public void RequireValid(CareerPathSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathSchedule> items)
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

/// <summary>Functions of career path schedule rules.</summary>
public static class CareerPathScheduleRules
{
    public static CareerPathScheduleValidator DefaultCareerPathScheduleValidator()
    {
        return new CareerPathScheduleValidator(
            new List<ICareerPathScheduleRule>
            {
                new CareerPathScheduleAllowedSegmentRule(new List<string> { "seasonal" }),
                new CareerPathScheduleSegmentLengthRule(32),
                new CareerPathScheduleRequiredContactEmailRule("career path schedule needs an entry"),
                new CareerPathScheduleRevisionRangeRule(120, 120),
                new CareerPathScheduleRequiredSegmentRule("career path schedule needs this field"),
                new CareerPathScheduleRequiredCodeRule("career path schedule needs an entry"),
            });
    }
}
