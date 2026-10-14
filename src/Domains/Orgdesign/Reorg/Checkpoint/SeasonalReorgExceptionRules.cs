namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>A rule a seasonal reorg exception must pass. Check returns its problems, or none.</summary>
public interface ISeasonalReorgExceptionRule
{
    List<string> Check(SeasonalReorgException item);
}

public sealed record SeasonalReorgExceptionLeaveDaysRangeRule(int Min, int Max) : ISeasonalReorgExceptionRule
{
    public List<string> Check(SeasonalReorgException item)
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

public sealed record SeasonalReorgExceptionBudgetRangeRule(double Min, double Max) : ISeasonalReorgExceptionRule
{
    public List<string> Check(SeasonalReorgException item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReorgExceptionRequiredLabelRule(string Message) : ISeasonalReorgExceptionRule
{
    public List<string> Check(SeasonalReorgException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal reorg exception.</summary>
public sealed record SeasonalReorgExceptionValidator(List<ISeasonalReorgExceptionRule> Rules)
{
    public List<string> Validate(SeasonalReorgException item)
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

    public void RequireValid(SeasonalReorgException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalReorgException> items)
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

/// <summary>Functions of seasonal reorg exception rules.</summary>
public static class SeasonalReorgExceptionRules
{
    public static SeasonalReorgExceptionValidator DefaultSeasonalReorgExceptionValidator()
    {
        return new SeasonalReorgExceptionValidator(
            new List<ISeasonalReorgExceptionRule>
            {
                new SeasonalReorgExceptionLeaveDaysRangeRule(64, 120),
                new SeasonalReorgExceptionBudgetRangeRule(6.0, 1592.4),
                new SeasonalReorgExceptionRequiredLabelRule("seasonal reorg exception needs an entry"),
            });
    }
}
