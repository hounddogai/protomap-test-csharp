namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>A rule a weighted holiday mapping must pass. Check returns its problems, or none.</summary>
public interface IWeightedHolidayMappingRule
{
    List<string> Check(WeightedHolidayMapping item);
}

public sealed record WeightedHolidayMappingNameLengthRule(int MaxLength) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHolidayMappingRequiredNameRule(string Message) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHolidayMappingPhoneLengthRule(int MaxLength) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedHolidayMappingRequiredSkillRule(string Message) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedHolidayMappingTenureMonthsRangeRule(int Min, int Max) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedHolidayMappingApprovedGuardRule(int Limit) : IWeightedHolidayMappingRule
{
    public List<string> Check(WeightedHolidayMapping item)
    {
        if (item.Approved && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted holiday mapping.</summary>
public sealed record WeightedHolidayMappingValidator(List<IWeightedHolidayMappingRule> Rules)
{
    public List<string> Validate(WeightedHolidayMapping item)
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

    public void RequireValid(WeightedHolidayMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedHolidayMapping> items)
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

/// <summary>Functions of weighted holiday mapping rules.</summary>
public static class WeightedHolidayMappingRules
{
    public static WeightedHolidayMappingValidator DefaultWeightedHolidayMappingValidator()
    {
        return new WeightedHolidayMappingValidator(
            new List<IWeightedHolidayMappingRule>
            {
                new WeightedHolidayMappingNameLengthRule(3600),
                new WeightedHolidayMappingRequiredNameRule("weighted holiday mapping needs an entry"),
                new WeightedHolidayMappingPhoneLengthRule(1000),
                new WeightedHolidayMappingRequiredSkillRule("weighted holiday mapping needs this field"),
                new WeightedHolidayMappingTenureMonthsRangeRule(64, 3600),
                new WeightedHolidayMappingApprovedGuardRule(255),
            });
    }
}
