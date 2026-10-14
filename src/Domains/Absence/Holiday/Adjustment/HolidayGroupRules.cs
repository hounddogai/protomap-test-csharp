namespace Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>A rule a holiday group must pass. Check returns its problems, or none.</summary>
public interface IHolidayGroupRule
{
    List<string> Check(HolidayGroup item);
}

public sealed record HolidayGroupSegmentLengthRule(int MaxLength) : IHolidayGroupRule
{
    public List<string> Check(HolidayGroup item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayGroupRequiredNameRule(string Message) : IHolidayGroupRule
{
    public List<string> Check(HolidayGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayGroupRequiredTitleRule(string Message) : IHolidayGroupRule
{
    public List<string> Check(HolidayGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday group.</summary>
public sealed record HolidayGroupValidator(List<IHolidayGroupRule> Rules)
{
    public List<string> Validate(HolidayGroup item)
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

    public void RequireValid(HolidayGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayGroup> items)
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

/// <summary>Functions of holiday group rules.</summary>
public static class HolidayGroupRules
{
    public static HolidayGroupValidator DefaultHolidayGroupValidator()
    {
        return new HolidayGroupValidator(
            new List<IHolidayGroupRule>
            {
                new HolidayGroupSegmentLengthRule(3600),
                new HolidayGroupRequiredNameRule("holiday group needs this field"),
                new HolidayGroupRequiredTitleRule("holiday group needs an entry"),
            });
    }
}
