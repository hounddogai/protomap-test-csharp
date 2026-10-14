namespace Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>A rule a lenient holiday adjustment must pass. Check returns its problems, or none.</summary>
public interface ILenientHolidayAdjustmentRule
{
    List<string> Check(LenientHolidayAdjustment item);
}

public sealed record LenientHolidayAdjustmentRequiredCodeRule(string Message) : ILenientHolidayAdjustmentRule
{
    public List<string> Check(LenientHolidayAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayAdjustmentRequiredCategoryRule(string Message) : ILenientHolidayAdjustmentRule
{
    public List<string> Check(LenientHolidayAdjustment item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayAdjustmentRequiredNotesRule(string Message) : ILenientHolidayAdjustmentRule
{
    public List<string> Check(LenientHolidayAdjustment item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayAdjustmentScoreRangeRule(double Min, double Max) : ILenientHolidayAdjustmentRule
{
    public List<string> Check(LenientHolidayAdjustment item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayAdjustmentRequiredNameRule(string Message) : ILenientHolidayAdjustmentRule
{
    public List<string> Check(LenientHolidayAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient holiday adjustment.</summary>
public sealed record LenientHolidayAdjustmentValidator(List<ILenientHolidayAdjustmentRule> Rules)
{
    public List<string> Validate(LenientHolidayAdjustment item)
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

    public void RequireValid(LenientHolidayAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientHolidayAdjustment> items)
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

/// <summary>Functions of lenient holiday adjustment rules.</summary>
public static class LenientHolidayAdjustmentRules
{
    public static LenientHolidayAdjustmentValidator DefaultLenientHolidayAdjustmentValidator()
    {
        return new LenientHolidayAdjustmentValidator(
            new List<ILenientHolidayAdjustmentRule>
            {
                new LenientHolidayAdjustmentRequiredCodeRule("lenient holiday adjustment needs this field"),
                new LenientHolidayAdjustmentRequiredCategoryRule("lenient holiday adjustment needs this field"),
                new LenientHolidayAdjustmentRequiredNotesRule("lenient holiday adjustment needs a value"),
                new LenientHolidayAdjustmentScoreRangeRule(6.0, 3880.4),
                new LenientHolidayAdjustmentRequiredNameRule("lenient holiday adjustment needs an entry"),
            });
    }
}
