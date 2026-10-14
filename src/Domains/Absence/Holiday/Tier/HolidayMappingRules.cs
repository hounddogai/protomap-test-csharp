namespace Acme.Workforce.Absence.Holiday.Tier;

/// <summary>A rule a holiday mapping must pass. Check returns its problems, or none.</summary>
public interface IHolidayMappingRule
{
    List<string> Check(HolidayMapping item);
}

public sealed record HolidayMappingEndsAtRangeRule(int Min, int Max) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMappingRequiredCategoryRule(string Message) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayMappingRequiredLabelRule(string Message) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayMappingCategoryLengthRule(int MaxLength) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMappingCreatedAtRangeRule(int Min, int Max) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayMappingLabelLengthRule(int MaxLength) : IHolidayMappingRule
{
    public List<string> Check(HolidayMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday mapping.</summary>
public sealed record HolidayMappingValidator(List<IHolidayMappingRule> Rules)
{
    public List<string> Validate(HolidayMapping item)
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

    public void RequireValid(HolidayMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayMapping> items)
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

/// <summary>Functions of holiday mapping rules.</summary>
public static class HolidayMappingRules
{
    public static HolidayMappingValidator DefaultHolidayMappingValidator()
    {
        return new HolidayMappingValidator(
            new List<IHolidayMappingRule>
            {
                new HolidayMappingEndsAtRangeRule(64, 255),
                new HolidayMappingRequiredCategoryRule("holiday mapping needs a value"),
                new HolidayMappingRequiredLabelRule("holiday mapping needs an entry"),
                new HolidayMappingCategoryLengthRule(64),
                new HolidayMappingCreatedAtRangeRule(16, 120),
                new HolidayMappingLabelLengthRule(16),
            });
    }
}
