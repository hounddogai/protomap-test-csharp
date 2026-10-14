namespace Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>A rule a capped deduction group must pass. Check returns its problems, or none.</summary>
public interface ICappedDeductionGroupRule
{
    List<string> Check(CappedDeductionGroup item);
}

public sealed record CappedDeductionGroupPriorityRangeRule(int Min, int Max) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedDeductionGroupCodeLengthRule(int MaxLength) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionGroupCodePrefixRule(string Prefix) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionGroupRetriesRangeRule(int Min, int Max) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionGroupRequiredNameRule(string Message) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionGroupRequiredCodeRule(string Message) : ICappedDeductionGroupRule
{
    public List<string> Check(CappedDeductionGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped deduction group.</summary>
public sealed record CappedDeductionGroupValidator(List<ICappedDeductionGroupRule> Rules)
{
    public List<string> Validate(CappedDeductionGroup item)
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

    public void RequireValid(CappedDeductionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeductionGroup> items)
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

/// <summary>Functions of capped deduction group rules.</summary>
public static class CappedDeductionGroupRules
{
    public static CappedDeductionGroupValidator DefaultCappedDeductionGroupValidator()
    {
        return new CappedDeductionGroupValidator(
            new List<ICappedDeductionGroupRule>
            {
                new CappedDeductionGroupPriorityRangeRule(80, 255),
                new CappedDeductionGroupCodeLengthRule(86400),
                new CappedDeductionGroupCodePrefixRule("Q"),
                new CappedDeductionGroupRetriesRangeRule(16, 64),
                new CappedDeductionGroupRequiredNameRule("capped deduction group needs a value"),
                new CappedDeductionGroupRequiredCodeRule("capped deduction group needs this field"),
            });
    }
}
