namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>A rule a seasonal cost model allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCostModelAllocationRule
{
    List<string> Check(SeasonalCostModelAllocation item);
}

public sealed record SeasonalCostModelAllocationLeaveDaysRangeRule(int Min, int Max) : ISeasonalCostModelAllocationRule
{
    public List<string> Check(SeasonalCostModelAllocation item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalCostModelAllocationCodeLengthRule(int MaxLength) : ISeasonalCostModelAllocationRule
{
    public List<string> Check(SeasonalCostModelAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelAllocationLabelLengthRule(int MaxLength) : ISeasonalCostModelAllocationRule
{
    public List<string> Check(SeasonalCostModelAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelAllocationWindowDaysRangeRule(int Min, int Max) : ISeasonalCostModelAllocationRule
{
    public List<string> Check(SeasonalCostModelAllocation item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal cost model allocation.</summary>
public sealed record SeasonalCostModelAllocationValidator(List<ISeasonalCostModelAllocationRule> Rules)
{
    public List<string> Validate(SeasonalCostModelAllocation item)
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

    public void RequireValid(SeasonalCostModelAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCostModelAllocation> items)
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

/// <summary>Functions of seasonal cost model allocation rules.</summary>
public static class SeasonalCostModelAllocationRules
{
    public static SeasonalCostModelAllocationValidator DefaultSeasonalCostModelAllocationValidator()
    {
        return new SeasonalCostModelAllocationValidator(
            new List<ISeasonalCostModelAllocationRule>
            {
                new SeasonalCostModelAllocationLeaveDaysRangeRule(64, 255),
                new SeasonalCostModelAllocationCodeLengthRule(1000),
                new SeasonalCostModelAllocationLabelLengthRule(3600),
                new SeasonalCostModelAllocationWindowDaysRangeRule(16, 32),
            });
    }
}
