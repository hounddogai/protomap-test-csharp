namespace Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>A rule a reorg rollup must pass. Check returns its problems, or none.</summary>
public interface IReorgRollupRule
{
    List<string> Check(ReorgRollup item);
}

public sealed record ReorgRollupRequiredCodeRule(string Message) : IReorgRollupRule
{
    public List<string> Check(ReorgRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgRollupNameLengthRule(int MaxLength) : IReorgRollupRule
{
    public List<string> Check(ReorgRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRollupCodePrefixRule(string Prefix) : IReorgRollupRule
{
    public List<string> Check(ReorgRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg rollup.</summary>
public sealed record ReorgRollupValidator(List<IReorgRollupRule> Rules)
{
    public List<string> Validate(ReorgRollup item)
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

    public void RequireValid(ReorgRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgRollup> items)
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

/// <summary>Functions of reorg rollup rules.</summary>
public static class ReorgRollupRules
{
    public static ReorgRollupValidator DefaultReorgRollupValidator()
    {
        return new ReorgRollupValidator(
            new List<IReorgRollupRule>
            {
                new ReorgRollupRequiredCodeRule("reorg rollup needs this field"),
                new ReorgRollupNameLengthRule(3600),
                new ReorgRollupCodePrefixRule("OPS"),
            });
    }
}
