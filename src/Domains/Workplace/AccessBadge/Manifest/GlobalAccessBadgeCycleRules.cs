namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global access badge cycle must pass. Check returns its problems, or none.</summary>
public interface IGlobalAccessBadgeCycleRule
{
    List<string> Check(GlobalAccessBadgeCycle item);
}

public sealed record GlobalAccessBadgeCycleTeamLengthRule(int MaxLength) : IGlobalAccessBadgeCycleRule
{
    public List<string> Check(GlobalAccessBadgeCycle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeCycleActiveGuardRule(double Limit) : IGlobalAccessBadgeCycleRule
{
    public List<string> Check(GlobalAccessBadgeCycle item)
    {
        if (item.Active && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeCycleCostRangeRule(double Min, double Max) : IGlobalAccessBadgeCycleRule
{
    public List<string> Check(GlobalAccessBadgeCycle item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAccessBadgeCycleTitleLengthRule(int MaxLength) : IGlobalAccessBadgeCycleRule
{
    public List<string> Check(GlobalAccessBadgeCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAccessBadgeCycleAllowedTimezoneRule(List<string> Allowed) : IGlobalAccessBadgeCycleRule
{
    public List<string> Check(GlobalAccessBadgeCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global access badge cycle.</summary>
public sealed record GlobalAccessBadgeCycleValidator(List<IGlobalAccessBadgeCycleRule> Rules)
{
    public List<string> Validate(GlobalAccessBadgeCycle item)
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

    public void RequireValid(GlobalAccessBadgeCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAccessBadgeCycle> items)
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

/// <summary>Functions of global access badge cycle rules.</summary>
public static class GlobalAccessBadgeCycleRules
{
    public static GlobalAccessBadgeCycleValidator DefaultGlobalAccessBadgeCycleValidator()
    {
        return new GlobalAccessBadgeCycleValidator(
            new List<IGlobalAccessBadgeCycleRule>
            {
                new GlobalAccessBadgeCycleTeamLengthRule(255),
                new GlobalAccessBadgeCycleActiveGuardRule(9.2),
                new GlobalAccessBadgeCycleCostRangeRule(8.1, 697.0),
                new GlobalAccessBadgeCycleTitleLengthRule(64),
                new GlobalAccessBadgeCycleAllowedTimezoneRule(new List<string> { "seasonal", "weighted", "lenient" }),
            });
    }
}
