namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>A rule a headcount group must pass. Check returns its problems, or none.</summary>
public interface IHeadcountGroupRule
{
    List<string> Check(HeadcountGroup item);
}

public sealed record HeadcountGroupAmountRangeRule(double Min, double Max) : IHeadcountGroupRule
{
    public List<string> Check(HeadcountGroup item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountGroupCodeLengthRule(int MaxLength) : IHeadcountGroupRule
{
    public List<string> Check(HeadcountGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountGroupNameLengthRule(int MaxLength) : IHeadcountGroupRule
{
    public List<string> Check(HeadcountGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountGroupCodePrefixRule(string Prefix) : IHeadcountGroupRule
{
    public List<string> Check(HeadcountGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount group.</summary>
public sealed record HeadcountGroupValidator(List<IHeadcountGroupRule> Rules)
{
    public List<string> Validate(HeadcountGroup item)
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

    public void RequireValid(HeadcountGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountGroup> items)
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

/// <summary>Functions of headcount group rules.</summary>
public static class HeadcountGroupRules
{
    public static HeadcountGroupValidator DefaultHeadcountGroupValidator()
    {
        return new HeadcountGroupValidator(
            new List<IHeadcountGroupRule>
            {
                new HeadcountGroupAmountRangeRule(1.3, 1806.2),
                new HeadcountGroupCodeLengthRule(86400),
                new HeadcountGroupNameLengthRule(1000),
                new HeadcountGroupCodePrefixRule("REF"),
            });
    }
}
