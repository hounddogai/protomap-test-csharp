namespace Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>A rule a holiday rollup must pass. Check returns its problems, or none.</summary>
public interface IHolidayRollupRule
{
    List<string> Check(HolidayRollup item);
}

public sealed record HolidayRollupBatchSizeRangeRule(int Min, int Max) : IHolidayRollupRule
{
    public List<string> Check(HolidayRollup item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayRollupTenureMonthsRangeRule(int Min, int Max) : IHolidayRollupRule
{
    public List<string> Check(HolidayRollup item)
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

public sealed record HolidayRollupCreatedAtRangeRule(int Min, int Max) : IHolidayRollupRule
{
    public List<string> Check(HolidayRollup item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a holiday rollup.</summary>
public sealed record HolidayRollupValidator(List<IHolidayRollupRule> Rules)
{
    public List<string> Validate(HolidayRollup item)
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

    public void RequireValid(HolidayRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayRollup> items)
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

/// <summary>Functions of holiday rollup rules.</summary>
public static class HolidayRollupRules
{
    public static HolidayRollupValidator DefaultHolidayRollupValidator()
    {
        return new HolidayRollupValidator(
            new List<IHolidayRollupRule>
            {
                new HolidayRollupBatchSizeRangeRule(80, 255),
                new HolidayRollupTenureMonthsRangeRule(64, 3600),
                new HolidayRollupCreatedAtRangeRule(16, 1000),
            });
    }
}
