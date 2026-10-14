namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual cycle must pass. Check returns its problems, or none.</summary>
public interface IAccrualCycleRule
{
    List<string> Check(AccrualCycle item);
}

public sealed record AccrualCycleNameLengthRule(int MaxLength) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCycleAllowedNameRule(List<string> Allowed) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCycleRequiredNameRule(string Message) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualCycleRequiredCodeRule(string Message) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualCycleApprovedGuardRule(int Limit) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        if (item.Approved && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AccrualCycleCategoryLengthRule(int MaxLength) : IAccrualCycleRule
{
    public List<string> Check(AccrualCycle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual cycle.</summary>
public sealed record AccrualCycleValidator(List<IAccrualCycleRule> Rules)
{
    public List<string> Validate(AccrualCycle item)
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

    public void RequireValid(AccrualCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualCycle> items)
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

/// <summary>Functions of accrual cycle rules.</summary>
public static class AccrualCycleRules
{
    public static AccrualCycleValidator DefaultAccrualCycleValidator()
    {
        return new AccrualCycleValidator(
            new List<IAccrualCycleRule>
            {
                new AccrualCycleNameLengthRule(86400),
                new AccrualCycleAllowedNameRule(new List<string> { "batched" }),
                new AccrualCycleRequiredNameRule("accrual cycle needs this field"),
                new AccrualCycleRequiredCodeRule("accrual cycle needs this field"),
                new AccrualCycleApprovedGuardRule(64),
                new AccrualCycleCategoryLengthRule(86400),
            });
    }
}
