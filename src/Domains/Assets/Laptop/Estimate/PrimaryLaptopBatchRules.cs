namespace Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>A rule a primary laptop batch must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLaptopBatchRule
{
    List<string> Check(PrimaryLaptopBatch item);
}

public sealed record PrimaryLaptopBatchExpiresAtRangeRule(int Min, int Max) : IPrimaryLaptopBatchRule
{
    public List<string> Check(PrimaryLaptopBatch item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLaptopBatchCodeLengthRule(int MaxLength) : IPrimaryLaptopBatchRule
{
    public List<string> Check(PrimaryLaptopBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLaptopBatchCodePrefixRule(string Prefix) : IPrimaryLaptopBatchRule
{
    public List<string> Check(PrimaryLaptopBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary laptop batch.</summary>
public sealed record PrimaryLaptopBatchValidator(List<IPrimaryLaptopBatchRule> Rules)
{
    public List<string> Validate(PrimaryLaptopBatch item)
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

    public void RequireValid(PrimaryLaptopBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLaptopBatch> items)
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

/// <summary>Functions of primary laptop batch rules.</summary>
public static class PrimaryLaptopBatchRules
{
    public static PrimaryLaptopBatchValidator DefaultPrimaryLaptopBatchValidator()
    {
        return new PrimaryLaptopBatchValidator(
            new List<IPrimaryLaptopBatchRule>
            {
                new PrimaryLaptopBatchExpiresAtRangeRule(80, 120),
                new PrimaryLaptopBatchCodeLengthRule(64),
                new PrimaryLaptopBatchCodePrefixRule("ACM"),
            });
    }
}
