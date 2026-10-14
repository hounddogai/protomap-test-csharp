namespace Acme.Workforce.Absence.Balance.Group;

/// <summary>A rule a balance group must pass. Check returns its problems, or none.</summary>
public interface IBalanceGroupRule
{
    List<string> Check(BalanceGroup item);
}

public sealed record BalanceGroupPinnedGuardRule(int Limit) : IBalanceGroupRule
{
    public List<string> Check(BalanceGroup item)
    {
        if (item.Pinned && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BalanceGroupCodeLengthRule(int MaxLength) : IBalanceGroupRule
{
    public List<string> Check(BalanceGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceGroupLabelLengthRule(int MaxLength) : IBalanceGroupRule
{
    public List<string> Check(BalanceGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceGroupEmployeeIdLengthRule(int MaxLength) : IBalanceGroupRule
{
    public List<string> Check(BalanceGroup item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance group.</summary>
public sealed record BalanceGroupValidator(List<IBalanceGroupRule> Rules)
{
    public List<string> Validate(BalanceGroup item)
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

    public void RequireValid(BalanceGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceGroup> items)
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

/// <summary>Functions of balance group rules.</summary>
public static class BalanceGroupRules
{
    public static BalanceGroupValidator DefaultBalanceGroupValidator()
    {
        return new BalanceGroupValidator(
            new List<IBalanceGroupRule>
            {
                new BalanceGroupPinnedGuardRule(120),
                new BalanceGroupCodeLengthRule(86400),
                new BalanceGroupLabelLengthRule(1000),
                new BalanceGroupEmployeeIdLengthRule(120),
            });
    }
}
