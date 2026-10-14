namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>A rule a return plan notice must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanNoticeRule
{
    List<string> Check(ReturnPlanNotice item);
}

public sealed record ReturnPlanNoticeRequiredTitleRule(string Message) : IReturnPlanNoticeRule
{
    public List<string> Check(ReturnPlanNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanNoticeRequiredCodeRule(string Message) : IReturnPlanNoticeRule
{
    public List<string> Check(ReturnPlanNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanNoticeCapacityRangeRule(int Min, int Max) : IReturnPlanNoticeRule
{
    public List<string> Check(ReturnPlanNotice item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanNoticeBadgeLengthRule(int MaxLength) : IReturnPlanNoticeRule
{
    public List<string> Check(ReturnPlanNotice item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanNoticeHoursRangeRule(double Min, double Max) : IReturnPlanNoticeRule
{
    public List<string> Check(ReturnPlanNotice item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a return plan notice.</summary>
public sealed record ReturnPlanNoticeValidator(List<IReturnPlanNoticeRule> Rules)
{
    public List<string> Validate(ReturnPlanNotice item)
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

    public void RequireValid(ReturnPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanNotice> items)
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

/// <summary>Functions of return plan notice rules.</summary>
public static class ReturnPlanNoticeRules
{
    public static ReturnPlanNoticeValidator DefaultReturnPlanNoticeValidator()
    {
        return new ReturnPlanNoticeValidator(
            new List<IReturnPlanNoticeRule>
            {
                new ReturnPlanNoticeRequiredTitleRule("return plan notice needs this field"),
                new ReturnPlanNoticeRequiredCodeRule("return plan notice needs a value"),
                new ReturnPlanNoticeCapacityRangeRule(255, 1000),
                new ReturnPlanNoticeBadgeLengthRule(255),
                new ReturnPlanNoticeHoursRangeRule(3.8, 2524.4),
            });
    }
}
