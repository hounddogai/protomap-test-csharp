namespace Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>A rule a rolling accrual window must pass. Check returns its problems, or none.</summary>
public interface IRollingAccrualWindowRule
{
    List<string> Check(RollingAccrualWindow item);
}

public sealed record RollingAccrualWindowRequiredCodeRule(string Message) : IRollingAccrualWindowRule
{
    public List<string> Check(RollingAccrualWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualWindowDurationMinutesRangeRule(int Min, int Max) : IRollingAccrualWindowRule
{
    public List<string> Check(RollingAccrualWindow item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccrualWindowCodeLengthRule(int MaxLength) : IRollingAccrualWindowRule
{
    public List<string> Check(RollingAccrualWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualWindowRequiredUnitRule(string Message) : IRollingAccrualWindowRule
{
    public List<string> Check(RollingAccrualWindow item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling accrual window.</summary>
public sealed record RollingAccrualWindowValidator(List<IRollingAccrualWindowRule> Rules)
{
    public List<string> Validate(RollingAccrualWindow item)
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

    public void RequireValid(RollingAccrualWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccrualWindow> items)
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

/// <summary>Functions of rolling accrual window rules.</summary>
public static class RollingAccrualWindowRules
{
    public static RollingAccrualWindowValidator DefaultRollingAccrualWindowValidator()
    {
        return new RollingAccrualWindowValidator(
            new List<IRollingAccrualWindowRule>
            {
                new RollingAccrualWindowRequiredCodeRule("rolling accrual window needs a value"),
                new RollingAccrualWindowDurationMinutesRangeRule(1000, 86400),
                new RollingAccrualWindowCodeLengthRule(64),
                new RollingAccrualWindowRequiredUnitRule("rolling accrual window needs a value"),
            });
    }
}
