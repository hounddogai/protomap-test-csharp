namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>A rule a garnishment budget must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentBudgetRule
{
    List<string> Check(GarnishmentBudget item);
}

public sealed record GarnishmentBudgetRequiredNotesRule(string Message) : IGarnishmentBudgetRule
{
    public List<string> Check(GarnishmentBudget item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBudgetRequiredExternalRefRule(string Message) : IGarnishmentBudgetRule
{
    public List<string> Check(GarnishmentBudget item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentBudgetArchivedGuardRule(int Limit) : IGarnishmentBudgetRule
{
    public List<string> Check(GarnishmentBudget item)
    {
        if (item.Archived && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment budget.</summary>
public sealed record GarnishmentBudgetValidator(List<IGarnishmentBudgetRule> Rules)
{
    public List<string> Validate(GarnishmentBudget item)
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

    public void RequireValid(GarnishmentBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentBudget> items)
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

/// <summary>Functions of garnishment budget rules.</summary>
public static class GarnishmentBudgetRules
{
    public static GarnishmentBudgetValidator DefaultGarnishmentBudgetValidator()
    {
        return new GarnishmentBudgetValidator(
            new List<IGarnishmentBudgetRule>
            {
                new GarnishmentBudgetRequiredNotesRule("garnishment budget needs an entry"),
                new GarnishmentBudgetRequiredExternalRefRule("garnishment budget needs an entry"),
                new GarnishmentBudgetArchivedGuardRule(3600),
            });
    }
}
