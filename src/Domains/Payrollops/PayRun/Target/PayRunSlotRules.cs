namespace Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>A rule a pay run slot must pass. Check returns its problems, or none.</summary>
public interface IPayRunSlotRule
{
    List<string> Check(PayRunSlot item);
}

public sealed record PayRunSlotRequiredLabelRule(string Message) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunSlotRequiredCodeRule(string Message) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunSlotCodeLengthRule(int MaxLength) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSlotCodePrefixRule(string Prefix) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSlotCategoryLengthRule(int MaxLength) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSlotRequiredCategoryRule(string Message) : IPayRunSlotRule
{
    public List<string> Check(PayRunSlot item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run slot.</summary>
public sealed record PayRunSlotValidator(List<IPayRunSlotRule> Rules)
{
    public List<string> Validate(PayRunSlot item)
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

    public void RequireValid(PayRunSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunSlot> items)
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

/// <summary>Functions of pay run slot rules.</summary>
public static class PayRunSlotRules
{
    public static PayRunSlotValidator DefaultPayRunSlotValidator()
    {
        return new PayRunSlotValidator(
            new List<IPayRunSlotRule>
            {
                new PayRunSlotRequiredLabelRule("pay run slot needs a value"),
                new PayRunSlotRequiredCodeRule("pay run slot needs an entry"),
                new PayRunSlotCodeLengthRule(80),
                new PayRunSlotCodePrefixRule("Q"),
                new PayRunSlotCategoryLengthRule(86400),
                new PayRunSlotRequiredCategoryRule("pay run slot needs this field"),
            });
    }
}
