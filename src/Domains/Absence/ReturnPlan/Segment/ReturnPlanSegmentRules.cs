namespace Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>A rule a return plan segment must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanSegmentRule
{
    List<string> Check(ReturnPlanSegment item);
}

public sealed record ReturnPlanSegmentStartsAtRangeRule(int Min, int Max) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSegmentRequiredCodeRule(string Message) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSegmentNameLengthRule(int MaxLength) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSegmentGraceDaysRangeRule(int Min, int Max) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
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

public sealed record ReturnPlanSegmentFteRangeRule(double Min, double Max) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
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

public sealed record ReturnPlanSegmentCodeLengthRule(int MaxLength) : IReturnPlanSegmentRule
{
    public List<string> Check(ReturnPlanSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan segment.</summary>
public sealed record ReturnPlanSegmentValidator(List<IReturnPlanSegmentRule> Rules)
{
    public List<string> Validate(ReturnPlanSegment item)
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

    public void RequireValid(ReturnPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanSegment> items)
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

/// <summary>Functions of return plan segment rules.</summary>
public static class ReturnPlanSegmentRules
{
    public static ReturnPlanSegmentValidator DefaultReturnPlanSegmentValidator()
    {
        return new ReturnPlanSegmentValidator(
            new List<IReturnPlanSegmentRule>
            {
                new ReturnPlanSegmentStartsAtRangeRule(32, 86400),
                new ReturnPlanSegmentRequiredCodeRule("return plan segment needs a value"),
                new ReturnPlanSegmentNameLengthRule(3600),
                new ReturnPlanSegmentGraceDaysRangeRule(16, 64),
                new ReturnPlanSegmentFteRangeRule(5.9, 4388.2),
                new ReturnPlanSegmentCodeLengthRule(80),
            });
    }
}
