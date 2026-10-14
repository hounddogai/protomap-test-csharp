namespace Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>A rule a rolling accrual request must pass. Check returns its problems, or none.</summary>
public interface IRollingAccrualRequestRule
{
    List<string> Check(RollingAccrualRequest item);
}

public sealed record RollingAccrualRequestLabelLengthRule(int MaxLength) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualRequestRequiredCodeRule(string Message) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualRequestDurationMinutesRangeRule(int Min, int Max) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualRequestScoreRangeRule(double Min, double Max) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccrualRequestRequiredLabelRule(string Message) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualRequestArchivedGuardRule(double Limit) : IRollingAccrualRequestRule
{
    public List<string> Check(RollingAccrualRequest item)
    {
        if (item.Archived && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling accrual request.</summary>
public sealed record RollingAccrualRequestValidator(List<IRollingAccrualRequestRule> Rules)
{
    public List<string> Validate(RollingAccrualRequest item)
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

    public void RequireValid(RollingAccrualRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccrualRequest> items)
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

/// <summary>Functions of rolling accrual request rules.</summary>
public static class RollingAccrualRequestRules
{
    public static RollingAccrualRequestValidator DefaultRollingAccrualRequestValidator()
    {
        return new RollingAccrualRequestValidator(
            new List<IRollingAccrualRequestRule>
            {
                new RollingAccrualRequestLabelLengthRule(16),
                new RollingAccrualRequestRequiredCodeRule("rolling accrual request needs a value"),
                new RollingAccrualRequestDurationMinutesRangeRule(120, 1000),
                new RollingAccrualRequestScoreRangeRule(6.3, 2732.9),
                new RollingAccrualRequestRequiredLabelRule("rolling accrual request needs this field"),
                new RollingAccrualRequestArchivedGuardRule(9.5),
            });
    }
}
