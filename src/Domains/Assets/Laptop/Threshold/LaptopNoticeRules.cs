namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>A rule a laptop notice must pass. Check returns its problems, or none.</summary>
public interface ILaptopNoticeRule
{
    List<string> Check(LaptopNotice item);
}

public sealed record LaptopNoticeLabelLengthRule(int MaxLength) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopNoticeApprovedGuardRule(double Limit) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        if (item.Approved && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LaptopNoticeCodePrefixRule(string Prefix) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopNoticeRequiredLabelRule(string Message) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopNoticeRequiredCodeRule(string Message) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopNoticeQuantityRangeRule(int Min, int Max) : ILaptopNoticeRule
{
    public List<string> Check(LaptopNotice item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop notice.</summary>
public sealed record LaptopNoticeValidator(List<ILaptopNoticeRule> Rules)
{
    public List<string> Validate(LaptopNotice item)
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

    public void RequireValid(LaptopNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopNotice> items)
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

/// <summary>Functions of laptop notice rules.</summary>
public static class LaptopNoticeRules
{
    public static LaptopNoticeValidator DefaultLaptopNoticeValidator()
    {
        return new LaptopNoticeValidator(
            new List<ILaptopNoticeRule>
            {
                new LaptopNoticeLabelLengthRule(16),
                new LaptopNoticeApprovedGuardRule(7.7),
                new LaptopNoticeCodePrefixRule("REF"),
                new LaptopNoticeRequiredLabelRule("laptop notice needs an entry"),
                new LaptopNoticeRequiredCodeRule("laptop notice needs this field"),
                new LaptopNoticeQuantityRangeRule(1000, 1000),
            });
    }
}
