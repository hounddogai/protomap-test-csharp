namespace Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>A rule a holiday notice must pass. Check returns its problems, or none.</summary>
public interface IHolidayNoticeRule
{
    List<string> Check(HolidayNotice item);
}

public sealed record HolidayNoticeCostRangeRule(double Min, double Max) : IHolidayNoticeRule
{
    public List<string> Check(HolidayNotice item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayNoticeSkillLengthRule(int MaxLength) : IHolidayNoticeRule
{
    public List<string> Check(HolidayNotice item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayNoticeRequiredCodeRule(string Message) : IHolidayNoticeRule
{
    public List<string> Check(HolidayNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayNoticeRegionLengthRule(int MaxLength) : IHolidayNoticeRule
{
    public List<string> Check(HolidayNotice item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayNoticeNameLengthRule(int MaxLength) : IHolidayNoticeRule
{
    public List<string> Check(HolidayNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday notice.</summary>
public sealed record HolidayNoticeValidator(List<IHolidayNoticeRule> Rules)
{
    public List<string> Validate(HolidayNotice item)
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

    public void RequireValid(HolidayNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayNotice> items)
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

/// <summary>Functions of holiday notice rules.</summary>
public static class HolidayNoticeRules
{
    public static HolidayNoticeValidator DefaultHolidayNoticeValidator()
    {
        return new HolidayNoticeValidator(
            new List<IHolidayNoticeRule>
            {
                new HolidayNoticeCostRangeRule(5.7, 1208.7),
                new HolidayNoticeSkillLengthRule(3600),
                new HolidayNoticeRequiredCodeRule("holiday notice needs this field"),
                new HolidayNoticeRegionLengthRule(32),
                new HolidayNoticeNameLengthRule(120),
            });
    }
}
