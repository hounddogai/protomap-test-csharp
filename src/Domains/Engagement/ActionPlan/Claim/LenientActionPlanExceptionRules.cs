namespace Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>A rule a lenient action plan exception must pass. Check returns its problems, or none.</summary>
public interface ILenientActionPlanExceptionRule
{
    List<string> Check(LenientActionPlanException item);
}

public sealed record LenientActionPlanExceptionRequiredCodeRule(string Message) : ILenientActionPlanExceptionRule
{
    public List<string> Check(LenientActionPlanException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanExceptionRequiredLocationRule(string Message) : ILenientActionPlanExceptionRule
{
    public List<string> Check(LenientActionPlanException item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanExceptionRatingRangeRule(double Min, double Max) : ILenientActionPlanExceptionRule
{
    public List<string> Check(LenientActionPlanException item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanExceptionLocationLengthRule(int MaxLength) : ILenientActionPlanExceptionRule
{
    public List<string> Check(LenientActionPlanException item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient action plan exception.</summary>
public sealed record LenientActionPlanExceptionValidator(List<ILenientActionPlanExceptionRule> Rules)
{
    public List<string> Validate(LenientActionPlanException item)
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

    public void RequireValid(LenientActionPlanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientActionPlanException> items)
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

/// <summary>Functions of lenient action plan exception rules.</summary>
public static class LenientActionPlanExceptionRules
{
    public static LenientActionPlanExceptionValidator DefaultLenientActionPlanExceptionValidator()
    {
        return new LenientActionPlanExceptionValidator(
            new List<ILenientActionPlanExceptionRule>
            {
                new LenientActionPlanExceptionRequiredCodeRule("lenient action plan exception needs a value"),
                new LenientActionPlanExceptionRequiredLocationRule("lenient action plan exception needs this field"),
                new LenientActionPlanExceptionRatingRangeRule(4.4, 2520.4),
                new LenientActionPlanExceptionLocationLengthRule(16),
            });
    }
}
