namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

/// <summary>A rule a seasonal accrual batch must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccrualBatchRule
{
    List<string> Check(SeasonalAccrualBatch item);
}

public sealed record SeasonalAccrualBatchLeaveDaysRangeRule(int Min, int Max) : ISeasonalAccrualBatchRule
{
    public List<string> Check(SeasonalAccrualBatch item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualBatchCodeLengthRule(int MaxLength) : ISeasonalAccrualBatchRule
{
    public List<string> Check(SeasonalAccrualBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualBatchMarginRangeRule(double Min, double Max) : ISeasonalAccrualBatchRule
{
    public List<string> Check(SeasonalAccrualBatch item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal accrual batch.</summary>
public sealed record SeasonalAccrualBatchValidator(List<ISeasonalAccrualBatchRule> Rules)
{
    public List<string> Validate(SeasonalAccrualBatch item)
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

    public void RequireValid(SeasonalAccrualBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccrualBatch> items)
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

/// <summary>Functions of seasonal accrual batch rules.</summary>
public static class SeasonalAccrualBatchRules
{
    public static SeasonalAccrualBatchValidator DefaultSeasonalAccrualBatchValidator()
    {
        return new SeasonalAccrualBatchValidator(
            new List<ISeasonalAccrualBatchRule>
            {
                new SeasonalAccrualBatchLeaveDaysRangeRule(16, 64),
                new SeasonalAccrualBatchCodeLengthRule(86400),
                new SeasonalAccrualBatchMarginRangeRule(5.0, 311.6),
            });
    }
}
