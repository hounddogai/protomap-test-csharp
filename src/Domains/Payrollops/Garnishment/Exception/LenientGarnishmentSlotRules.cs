namespace Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>A rule a lenient garnishment slot must pass. Check returns its problems, or none.</summary>
public interface ILenientGarnishmentSlotRule
{
    List<string> Check(LenientGarnishmentSlot item);
}

public sealed record LenientGarnishmentSlotDiscountRangeRule(double Min, double Max) : ILenientGarnishmentSlotRule
{
    public List<string> Check(LenientGarnishmentSlot item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientGarnishmentSlotRequiredTitleRule(string Message) : ILenientGarnishmentSlotRule
{
    public List<string> Check(LenientGarnishmentSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentSlotTitleLengthRule(int MaxLength) : ILenientGarnishmentSlotRule
{
    public List<string> Check(LenientGarnishmentSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentSlotRequiredCodeRule(string Message) : ILenientGarnishmentSlotRule
{
    public List<string> Check(LenientGarnishmentSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentSlotCodePrefixRule(string Prefix) : ILenientGarnishmentSlotRule
{
    public List<string> Check(LenientGarnishmentSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient garnishment slot.</summary>
public sealed record LenientGarnishmentSlotValidator(List<ILenientGarnishmentSlotRule> Rules)
{
    public List<string> Validate(LenientGarnishmentSlot item)
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

    public void RequireValid(LenientGarnishmentSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientGarnishmentSlot> items)
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

/// <summary>Functions of lenient garnishment slot rules.</summary>
public static class LenientGarnishmentSlotRules
{
    public static LenientGarnishmentSlotValidator DefaultLenientGarnishmentSlotValidator()
    {
        return new LenientGarnishmentSlotValidator(
            new List<ILenientGarnishmentSlotRule>
            {
                new LenientGarnishmentSlotDiscountRangeRule(9.4, 431.0),
                new LenientGarnishmentSlotRequiredTitleRule("lenient garnishment slot needs a value"),
                new LenientGarnishmentSlotTitleLengthRule(1000),
                new LenientGarnishmentSlotRequiredCodeRule("lenient garnishment slot needs this field"),
                new LenientGarnishmentSlotCodePrefixRule("X"),
            });
    }
}
