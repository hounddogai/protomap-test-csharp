namespace Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>A rule a tiered accrual score must pass. Check returns its problems, or none.</summary>
public interface ITieredAccrualScoreRule
{
    List<string> Check(TieredAccrualScore item);
}

public sealed record TieredAccrualScoreRequiredCodeRule(string Message) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualScoreFteRangeRule(double Min, double Max) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualScoreRequiredTitleRule(string Message) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualScoreHoursRangeRule(double Min, double Max) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredAccrualScoreLabelLengthRule(int MaxLength) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualScoreRequiredNameRule(string Message) : ITieredAccrualScoreRule
{
    public List<string> Check(TieredAccrualScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered accrual score.</summary>
public sealed record TieredAccrualScoreValidator(List<ITieredAccrualScoreRule> Rules)
{
    public List<string> Validate(TieredAccrualScore item)
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

    public void RequireValid(TieredAccrualScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAccrualScore> items)
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

/// <summary>Functions of tiered accrual score rules.</summary>
public static class TieredAccrualScoreRules
{
    public static TieredAccrualScoreValidator DefaultTieredAccrualScoreValidator()
    {
        return new TieredAccrualScoreValidator(
            new List<ITieredAccrualScoreRule>
            {
                new TieredAccrualScoreRequiredCodeRule("tiered accrual score needs an entry"),
                new TieredAccrualScoreFteRangeRule(8.6, 4341.7),
                new TieredAccrualScoreRequiredTitleRule("tiered accrual score needs this field"),
                new TieredAccrualScoreHoursRangeRule(5.4, 1969.8),
                new TieredAccrualScoreLabelLengthRule(16),
                new TieredAccrualScoreRequiredNameRule("tiered accrual score needs this field"),
            });
    }
}
