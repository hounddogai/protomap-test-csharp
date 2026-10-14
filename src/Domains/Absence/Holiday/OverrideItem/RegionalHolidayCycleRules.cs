namespace Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>A rule a regional holiday cycle must pass. Check returns its problems, or none.</summary>
public interface IRegionalHolidayCycleRule
{
    List<string> Check(RegionalHolidayCycle item);
}

public sealed record RegionalHolidayCycleSalaryBandLengthRule(int MaxLength) : IRegionalHolidayCycleRule
{
    public List<string> Check(RegionalHolidayCycle item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayCycleBudgetRangeRule(double Min, double Max) : IRegionalHolidayCycleRule
{
    public List<string> Check(RegionalHolidayCycle item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayCycleNameLengthRule(int MaxLength) : IRegionalHolidayCycleRule
{
    public List<string> Check(RegionalHolidayCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayCyclePriorityRangeRule(int Min, int Max) : IRegionalHolidayCycleRule
{
    public List<string> Check(RegionalHolidayCycle item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional holiday cycle.</summary>
public sealed record RegionalHolidayCycleValidator(List<IRegionalHolidayCycleRule> Rules)
{
    public List<string> Validate(RegionalHolidayCycle item)
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

    public void RequireValid(RegionalHolidayCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalHolidayCycle> items)
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

/// <summary>Functions of regional holiday cycle rules.</summary>
public static class RegionalHolidayCycleRules
{
    public static RegionalHolidayCycleValidator DefaultRegionalHolidayCycleValidator()
    {
        return new RegionalHolidayCycleValidator(
            new List<IRegionalHolidayCycleRule>
            {
                new RegionalHolidayCycleSalaryBandLengthRule(3600),
                new RegionalHolidayCycleBudgetRangeRule(4.7, 4087.4),
                new RegionalHolidayCycleNameLengthRule(16),
                new RegionalHolidayCyclePriorityRangeRule(16, 3600),
            });
    }
}
