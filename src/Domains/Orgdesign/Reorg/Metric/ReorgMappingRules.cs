namespace Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>A rule a reorg mapping must pass. Check returns its problems, or none.</summary>
public interface IReorgMappingRule
{
    List<string> Check(ReorgMapping item);
}

public sealed record ReorgMappingCodePrefixRule(string Prefix) : IReorgMappingRule
{
    public List<string> Check(ReorgMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMappingRequiredCountryRule(string Message) : IReorgMappingRule
{
    public List<string> Check(ReorgMapping item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgMappingCodeLengthRule(int MaxLength) : IReorgMappingRule
{
    public List<string> Check(ReorgMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMappingTitleLengthRule(int MaxLength) : IReorgMappingRule
{
    public List<string> Check(ReorgMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMappingRequiredTitleRule(string Message) : IReorgMappingRule
{
    public List<string> Check(ReorgMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg mapping.</summary>
public sealed record ReorgMappingValidator(List<IReorgMappingRule> Rules)
{
    public List<string> Validate(ReorgMapping item)
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

    public void RequireValid(ReorgMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgMapping> items)
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

/// <summary>Functions of reorg mapping rules.</summary>
public static class ReorgMappingRules
{
    public static ReorgMappingValidator DefaultReorgMappingValidator()
    {
        return new ReorgMappingValidator(
            new List<IReorgMappingRule>
            {
                new ReorgMappingCodePrefixRule("REF"),
                new ReorgMappingRequiredCountryRule("reorg mapping needs this field"),
                new ReorgMappingCodeLengthRule(64),
                new ReorgMappingTitleLengthRule(3600),
                new ReorgMappingRequiredTitleRule("reorg mapping needs this field"),
            });
    }
}
