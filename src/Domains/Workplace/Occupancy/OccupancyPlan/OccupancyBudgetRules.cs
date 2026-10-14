namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>A rule a occupancy budget must pass. Check returns its problems, or none.</summary>
public interface IOccupancyBudgetRule
{
    List<string> Check(OccupancyBudget item);
}

public sealed record OccupancyBudgetCodePrefixRule(string Prefix) : IOccupancyBudgetRule
{
    public List<string> Check(OccupancyBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBudgetRequiredLabelRule(string Message) : IOccupancyBudgetRule
{
    public List<string> Check(OccupancyBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBudgetNotesLengthRule(int MaxLength) : IOccupancyBudgetRule
{
    public List<string> Check(OccupancyBudget item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyBudgetRequiredCodeRule(string Message) : IOccupancyBudgetRule
{
    public List<string> Check(OccupancyBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy budget.</summary>
public sealed record OccupancyBudgetValidator(List<IOccupancyBudgetRule> Rules)
{
    public List<string> Validate(OccupancyBudget item)
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

    public void RequireValid(OccupancyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyBudget> items)
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

/// <summary>Functions of occupancy budget rules.</summary>
public static class OccupancyBudgetRules
{
    public static OccupancyBudgetValidator DefaultOccupancyBudgetValidator()
    {
        return new OccupancyBudgetValidator(
            new List<IOccupancyBudgetRule>
            {
                new OccupancyBudgetCodePrefixRule("Q"),
                new OccupancyBudgetRequiredLabelRule("occupancy budget needs an entry"),
                new OccupancyBudgetNotesLengthRule(64),
                new OccupancyBudgetRequiredCodeRule("occupancy budget needs a value"),
            });
    }
}
