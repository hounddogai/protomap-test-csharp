namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>A rule a capped laptop record must pass. Check returns its problems, or none.</summary>
public interface ICappedLaptopRecordRule
{
    List<string> Check(CappedLaptopRecord item);
}

public sealed record CappedLaptopRecordNameLengthRule(int MaxLength) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRecordExpiresAtRangeRule(int Min, int Max) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedLaptopRecordCodePrefixRule(string Prefix) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRecordCodeLengthRule(int MaxLength) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRecordRequiredNameRule(string Message) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRecordDiscountRangeRule(double Min, double Max) : ICappedLaptopRecordRule
{
    public List<string> Check(CappedLaptopRecord item)
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

/// <summary>Runs every rule of a capped laptop record.</summary>
public sealed record CappedLaptopRecordValidator(List<ICappedLaptopRecordRule> Rules)
{
    public List<string> Validate(CappedLaptopRecord item)
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

    public void RequireValid(CappedLaptopRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLaptopRecord> items)
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

/// <summary>Functions of capped laptop record rules.</summary>
public static class CappedLaptopRecordRules
{
    public static CappedLaptopRecordValidator DefaultCappedLaptopRecordValidator()
    {
        return new CappedLaptopRecordValidator(
            new List<ICappedLaptopRecordRule>
            {
                new CappedLaptopRecordNameLengthRule(32),
                new CappedLaptopRecordExpiresAtRangeRule(120, 86400),
                new CappedLaptopRecordCodePrefixRule("ACM"),
                new CappedLaptopRecordCodeLengthRule(86400),
                new CappedLaptopRecordRequiredNameRule("capped laptop record needs an entry"),
                new CappedLaptopRecordDiscountRangeRule(5.8, 4611.9),
            });
    }
}
