namespace Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>A rule a strict visitor slot must pass. Check returns its problems, or none.</summary>
public interface IStrictVisitorSlotRule
{
    List<string> Check(StrictVisitorSlot item);
}

public sealed record StrictVisitorSlotRequiredCategoryRule(string Message) : IStrictVisitorSlotRule
{
    public List<string> Check(StrictVisitorSlot item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictVisitorSlotCategoryLengthRule(int MaxLength) : IStrictVisitorSlotRule
{
    public List<string> Check(StrictVisitorSlot item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictVisitorSlotNameLengthRule(int MaxLength) : IStrictVisitorSlotRule
{
    public List<string> Check(StrictVisitorSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictVisitorSlotRateRangeRule(double Min, double Max) : IStrictVisitorSlotRule
{
    public List<string> Check(StrictVisitorSlot item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictVisitorSlotRatingRangeRule(double Min, double Max) : IStrictVisitorSlotRule
{
    public List<string> Check(StrictVisitorSlot item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict visitor slot.</summary>
public sealed record StrictVisitorSlotValidator(List<IStrictVisitorSlotRule> Rules)
{
    public List<string> Validate(StrictVisitorSlot item)
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

    public void RequireValid(StrictVisitorSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictVisitorSlot> items)
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

/// <summary>Functions of strict visitor slot rules.</summary>
public static class StrictVisitorSlotRules
{
    public static StrictVisitorSlotValidator DefaultStrictVisitorSlotValidator()
    {
        return new StrictVisitorSlotValidator(
            new List<IStrictVisitorSlotRule>
            {
                new StrictVisitorSlotRequiredCategoryRule("strict visitor slot needs a value"),
                new StrictVisitorSlotCategoryLengthRule(64),
                new StrictVisitorSlotNameLengthRule(16),
                new StrictVisitorSlotRateRangeRule(7.6, 2617.5),
                new StrictVisitorSlotRatingRangeRule(5.4, 2538.3),
            });
    }
}
