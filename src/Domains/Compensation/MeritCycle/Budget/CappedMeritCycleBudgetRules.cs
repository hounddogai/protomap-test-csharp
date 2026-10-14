namespace Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>A rule a capped merit cycle budget must pass. Check returns its problems, or none.</summary>
public interface ICappedMeritCycleBudgetRule
{
    List<string> Check(CappedMeritCycleBudget item);
}

public sealed record CappedMeritCycleBudgetCodePrefixRule(string Prefix) : ICappedMeritCycleBudgetRule
{
    public List<string> Check(CappedMeritCycleBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleBudgetArchivedGuardRule(int Limit) : ICappedMeritCycleBudgetRule
{
    public List<string> Check(CappedMeritCycleBudget item)
    {
        if (item.Archived && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleBudgetTeamLengthRule(int MaxLength) : ICappedMeritCycleBudgetRule
{
    public List<string> Check(CappedMeritCycleBudget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleBudgetPriorityRangeRule(int Min, int Max) : ICappedMeritCycleBudgetRule
{
    public List<string> Check(CappedMeritCycleBudget item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleBudgetNameLengthRule(int MaxLength) : ICappedMeritCycleBudgetRule
{
    public List<string> Check(CappedMeritCycleBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped merit cycle budget.</summary>
public sealed record CappedMeritCycleBudgetValidator(List<ICappedMeritCycleBudgetRule> Rules)
{
    public List<string> Validate(CappedMeritCycleBudget item)
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

    public void RequireValid(CappedMeritCycleBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedMeritCycleBudget> items)
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

/// <summary>Functions of capped merit cycle budget rules.</summary>
public static class CappedMeritCycleBudgetRules
{
    public static CappedMeritCycleBudgetValidator DefaultCappedMeritCycleBudgetValidator()
    {
        return new CappedMeritCycleBudgetValidator(
            new List<ICappedMeritCycleBudgetRule>
            {
                new CappedMeritCycleBudgetCodePrefixRule("OPS"),
                new CappedMeritCycleBudgetArchivedGuardRule(64),
                new CappedMeritCycleBudgetTeamLengthRule(255),
                new CappedMeritCycleBudgetPriorityRangeRule(255, 3600),
                new CappedMeritCycleBudgetNameLengthRule(80),
            });
    }
}
