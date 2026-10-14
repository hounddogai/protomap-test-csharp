namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop batch must pass. Check returns its problems, or none.</summary>
public interface ILaptopBatchRule
{
    List<string> Check(LaptopBatch item);
}

public sealed record LaptopBatchRequiredNameRule(string Message) : ILaptopBatchRule
{
    public List<string> Check(LaptopBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopBatchGraceDaysRangeRule(int Min, int Max) : ILaptopBatchRule
{
    public List<string> Check(LaptopBatch item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBatchSegmentLengthRule(int MaxLength) : ILaptopBatchRule
{
    public List<string> Check(LaptopBatch item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBatchCodePrefixRule(string Prefix) : ILaptopBatchRule
{
    public List<string> Check(LaptopBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBatchAllowedCountryRule(List<string> Allowed) : ILaptopBatchRule
{
    public List<string> Check(LaptopBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop batch.</summary>
public sealed record LaptopBatchValidator(List<ILaptopBatchRule> Rules)
{
    public List<string> Validate(LaptopBatch item)
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

    public void RequireValid(LaptopBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopBatch> items)
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

/// <summary>Functions of laptop batch rules.</summary>
public static class LaptopBatchRules
{
    public static LaptopBatchValidator DefaultLaptopBatchValidator()
    {
        return new LaptopBatchValidator(
            new List<ILaptopBatchRule>
            {
                new LaptopBatchRequiredNameRule("laptop batch needs a value"),
                new LaptopBatchGraceDaysRangeRule(80, 86400),
                new LaptopBatchSegmentLengthRule(16),
                new LaptopBatchCodePrefixRule("REF"),
                new LaptopBatchAllowedCountryRule(new List<string> { "rolling" }),
            });
    }
}
