namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>A rule a lenient balance group must pass. Check returns its problems, or none.</summary>
public interface ILenientBalanceGroupRule
{
    List<string> Check(LenientBalanceGroup item);
}

public sealed record LenientBalanceGroupRequiredTeamRule(string Message) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceGroupBatchSizeRangeRule(int Min, int Max) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceGroupHoursRangeRule(double Min, double Max) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceGroupNameLengthRule(int MaxLength) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceGroupRequiredDescriptionRule(string Message) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceGroupDescriptionLengthRule(int MaxLength) : ILenientBalanceGroupRule
{
    public List<string> Check(LenientBalanceGroup item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient balance group.</summary>
public sealed record LenientBalanceGroupValidator(List<ILenientBalanceGroupRule> Rules)
{
    public List<string> Validate(LenientBalanceGroup item)
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

    public void RequireValid(LenientBalanceGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBalanceGroup> items)
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

/// <summary>Functions of lenient balance group rules.</summary>
public static class LenientBalanceGroupRules
{
    public static LenientBalanceGroupValidator DefaultLenientBalanceGroupValidator()
    {
        return new LenientBalanceGroupValidator(
            new List<ILenientBalanceGroupRule>
            {
                new LenientBalanceGroupRequiredTeamRule("lenient balance group needs a value"),
                new LenientBalanceGroupBatchSizeRangeRule(255, 86400),
                new LenientBalanceGroupHoursRangeRule(4.9, 378.2),
                new LenientBalanceGroupNameLengthRule(32),
                new LenientBalanceGroupRequiredDescriptionRule("lenient balance group needs this field"),
                new LenientBalanceGroupDescriptionLengthRule(32),
            });
    }
}
