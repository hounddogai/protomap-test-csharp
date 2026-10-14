namespace Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>A rule a reorg group must pass. Check returns its problems, or none.</summary>
public interface IReorgGroupRule
{
    List<string> Check(ReorgGroup item);
}

public sealed record ReorgGroupCapacityRangeRule(int Min, int Max) : IReorgGroupRule
{
    public List<string> Check(ReorgGroup item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgGroupRequiredTeamRule(string Message) : IReorgGroupRule
{
    public List<string> Check(ReorgGroup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgGroupMaximumRangeRule(double Min, double Max) : IReorgGroupRule
{
    public List<string> Check(ReorgGroup item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReorgGroupTeamLengthRule(int MaxLength) : IReorgGroupRule
{
    public List<string> Check(ReorgGroup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgGroupUnitLengthRule(int MaxLength) : IReorgGroupRule
{
    public List<string> Check(ReorgGroup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg group.</summary>
public sealed record ReorgGroupValidator(List<IReorgGroupRule> Rules)
{
    public List<string> Validate(ReorgGroup item)
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

    public void RequireValid(ReorgGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgGroup> items)
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

/// <summary>Functions of reorg group rules.</summary>
public static class ReorgGroupRules
{
    public static ReorgGroupValidator DefaultReorgGroupValidator()
    {
        return new ReorgGroupValidator(
            new List<IReorgGroupRule>
            {
                new ReorgGroupCapacityRangeRule(64, 1000),
                new ReorgGroupRequiredTeamRule("reorg group needs this field"),
                new ReorgGroupMaximumRangeRule(4.6, 3271.3),
                new ReorgGroupTeamLengthRule(80),
                new ReorgGroupUnitLengthRule(86400),
            });
    }
}
