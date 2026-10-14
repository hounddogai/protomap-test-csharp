namespace Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>A rule a primary warranty profile must pass. Check returns its problems, or none.</summary>
public interface IPrimaryWarrantyProfileRule
{
    List<string> Check(PrimaryWarrantyProfile item);
}

public sealed record PrimaryWarrantyProfileChannelLengthRule(int MaxLength) : IPrimaryWarrantyProfileRule
{
    public List<string> Check(PrimaryWarrantyProfile item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryWarrantyProfileRatioRangeRule(double Min, double Max) : IPrimaryWarrantyProfileRule
{
    public List<string> Check(PrimaryWarrantyProfile item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryWarrantyProfileCodePrefixRule(string Prefix) : IPrimaryWarrantyProfileRule
{
    public List<string> Check(PrimaryWarrantyProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryWarrantyProfileRequiredCurrencyRule(string Message) : IPrimaryWarrantyProfileRule
{
    public List<string> Check(PrimaryWarrantyProfile item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryWarrantyProfileLabelLengthRule(int MaxLength) : IPrimaryWarrantyProfileRule
{
    public List<string> Check(PrimaryWarrantyProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary warranty profile.</summary>
public sealed record PrimaryWarrantyProfileValidator(List<IPrimaryWarrantyProfileRule> Rules)
{
    public List<string> Validate(PrimaryWarrantyProfile item)
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

    public void RequireValid(PrimaryWarrantyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryWarrantyProfile> items)
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

/// <summary>Functions of primary warranty profile rules.</summary>
public static class PrimaryWarrantyProfileRules
{
    public static PrimaryWarrantyProfileValidator DefaultPrimaryWarrantyProfileValidator()
    {
        return new PrimaryWarrantyProfileValidator(
            new List<IPrimaryWarrantyProfileRule>
            {
                new PrimaryWarrantyProfileChannelLengthRule(80),
                new PrimaryWarrantyProfileRatioRangeRule(4.4, 872.4),
                new PrimaryWarrantyProfileCodePrefixRule("REF"),
                new PrimaryWarrantyProfileRequiredCurrencyRule("primary warranty profile needs this field"),
                new PrimaryWarrantyProfileLabelLengthRule(16),
            });
    }
}
