namespace Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>A rule a deduction tier must pass. Check returns its problems, or none.</summary>
public interface IDeductionTierRule
{
    List<string> Check(DeductionTier item);
}

public sealed record DeductionTierRequiredTitleRule(string Message) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionTierCodePrefixRule(string Prefix) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionTierPhoneLengthRule(int MaxLength) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionTierRequiredPhoneRule(string Message) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionTierTitleLengthRule(int MaxLength) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionTierCodeLengthRule(int MaxLength) : IDeductionTierRule
{
    public List<string> Check(DeductionTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction tier.</summary>
public sealed record DeductionTierValidator(List<IDeductionTierRule> Rules)
{
    public List<string> Validate(DeductionTier item)
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

    public void RequireValid(DeductionTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionTier> items)
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

/// <summary>Functions of deduction tier rules.</summary>
public static class DeductionTierRules
{
    public static DeductionTierValidator DefaultDeductionTierValidator()
    {
        return new DeductionTierValidator(
            new List<IDeductionTierRule>
            {
                new DeductionTierRequiredTitleRule("deduction tier needs a value"),
                new DeductionTierCodePrefixRule("X"),
                new DeductionTierPhoneLengthRule(64),
                new DeductionTierRequiredPhoneRule("deduction tier needs a value"),
                new DeductionTierTitleLengthRule(120),
                new DeductionTierCodeLengthRule(120),
            });
    }
}
