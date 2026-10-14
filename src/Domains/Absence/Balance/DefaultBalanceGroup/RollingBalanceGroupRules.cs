namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>A rule a rolling balance group must pass. Check returns its problems, or none.</summary>
public interface IRollingBalanceGroupRule
{
    List<string> Check(RollingBalanceGroup item);
}

public sealed record RollingBalanceGroupRatioRangeRule(double Min, double Max) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceGroupSequenceRangeRule(int Min, int Max) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBalanceGroupCodePrefixRule(string Prefix) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceGroupCodeLengthRule(int MaxLength) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceGroupRequiredCodeRule(string Message) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceGroupUpdatedAtRangeRule(int Min, int Max) : IRollingBalanceGroupRule
{
    public List<string> Check(RollingBalanceGroup item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling balance group.</summary>
public sealed record RollingBalanceGroupValidator(List<IRollingBalanceGroupRule> Rules)
{
    public List<string> Validate(RollingBalanceGroup item)
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

    public void RequireValid(RollingBalanceGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBalanceGroup> items)
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

/// <summary>Functions of rolling balance group rules.</summary>
public static class RollingBalanceGroupRules
{
    public static RollingBalanceGroupValidator DefaultRollingBalanceGroupValidator()
    {
        return new RollingBalanceGroupValidator(
            new List<IRollingBalanceGroupRule>
            {
                new RollingBalanceGroupRatioRangeRule(2.5, 2993.2),
                new RollingBalanceGroupSequenceRangeRule(1000, 3600),
                new RollingBalanceGroupCodePrefixRule("REF"),
                new RollingBalanceGroupCodeLengthRule(120),
                new RollingBalanceGroupRequiredCodeRule("rolling balance group needs this field"),
                new RollingBalanceGroupUpdatedAtRangeRule(255, 3600),
            });
    }
}
