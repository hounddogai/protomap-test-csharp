namespace Acme.Workforce.Absence.Accrual.Template;

/// <summary>A rule a accrual milestone must pass. Check returns its problems, or none.</summary>
public interface IAccrualMilestoneRule
{
    List<string> Check(AccrualMilestone item);
}

public sealed record AccrualMilestonePinnedGuardRule(int Limit) : IAccrualMilestoneRule
{
    public List<string> Check(AccrualMilestone item)
    {
        if (item.Pinned && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record AccrualMilestoneNameLengthRule(int MaxLength) : IAccrualMilestoneRule
{
    public List<string> Check(AccrualMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualMilestoneRequiredNameRule(string Message) : IAccrualMilestoneRule
{
    public List<string> Check(AccrualMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual milestone.</summary>
public sealed record AccrualMilestoneValidator(List<IAccrualMilestoneRule> Rules)
{
    public List<string> Validate(AccrualMilestone item)
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

    public void RequireValid(AccrualMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualMilestone> items)
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

/// <summary>Functions of accrual milestone rules.</summary>
public static class AccrualMilestoneRules
{
    public static AccrualMilestoneValidator DefaultAccrualMilestoneValidator()
    {
        return new AccrualMilestoneValidator(
            new List<IAccrualMilestoneRule>
            {
                new AccrualMilestonePinnedGuardRule(32),
                new AccrualMilestoneNameLengthRule(80),
                new AccrualMilestoneRequiredNameRule("accrual milestone needs an entry"),
            });
    }
}
