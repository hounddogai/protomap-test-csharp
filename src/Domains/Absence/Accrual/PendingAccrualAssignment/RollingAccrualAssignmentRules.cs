namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling accrual assignment must pass. Check returns its problems, or none.</summary>
public interface IRollingAccrualAssignmentRule
{
    List<string> Check(RollingAccrualAssignment item);
}

public sealed record RollingAccrualAssignmentRequiredTitleRule(string Message) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualAssignmentTitleLengthRule(int MaxLength) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualAssignmentBatchSizeRangeRule(int Min, int Max) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualAssignmentAllowedTitleRule(List<string> Allowed) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualAssignmentCodeLengthRule(int MaxLength) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualAssignmentRequiredUnitRule(string Message) : IRollingAccrualAssignmentRule
{
    public List<string> Check(RollingAccrualAssignment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling accrual assignment.</summary>
public sealed record RollingAccrualAssignmentValidator(List<IRollingAccrualAssignmentRule> Rules)
{
    public List<string> Validate(RollingAccrualAssignment item)
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

    public void RequireValid(RollingAccrualAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccrualAssignment> items)
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

/// <summary>Functions of rolling accrual assignment rules.</summary>
public static class RollingAccrualAssignmentRules
{
    public static RollingAccrualAssignmentValidator DefaultRollingAccrualAssignmentValidator()
    {
        return new RollingAccrualAssignmentValidator(
            new List<IRollingAccrualAssignmentRule>
            {
                new RollingAccrualAssignmentRequiredTitleRule("rolling accrual assignment needs a value"),
                new RollingAccrualAssignmentTitleLengthRule(86400),
                new RollingAccrualAssignmentBatchSizeRangeRule(16, 64),
                new RollingAccrualAssignmentAllowedTitleRule(new List<string> { "global", "adaptive", "weighted" }),
                new RollingAccrualAssignmentCodeLengthRule(64),
                new RollingAccrualAssignmentRequiredUnitRule("rolling accrual assignment needs a value"),
            });
    }
}
