namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>A rule a default reorg exception must pass. Check returns its problems, or none.</summary>
public interface IDefaultReorgExceptionRule
{
    List<string> Check(DefaultReorgException item);
}

public sealed record DefaultReorgExceptionRequiredCategoryRule(string Message) : IDefaultReorgExceptionRule
{
    public List<string> Check(DefaultReorgException item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgExceptionVisibleGuardRule(double Limit) : IDefaultReorgExceptionRule
{
    public List<string> Check(DefaultReorgException item)
    {
        if (item.Visible && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgExceptionCodePrefixRule(string Prefix) : IDefaultReorgExceptionRule
{
    public List<string> Check(DefaultReorgException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default reorg exception.</summary>
public sealed record DefaultReorgExceptionValidator(List<IDefaultReorgExceptionRule> Rules)
{
    public List<string> Validate(DefaultReorgException item)
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

    public void RequireValid(DefaultReorgException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReorgException> items)
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

/// <summary>Functions of default reorg exception rules.</summary>
public static class DefaultReorgExceptionRules
{
    public static DefaultReorgExceptionValidator DefaultDefaultReorgExceptionValidator()
    {
        return new DefaultReorgExceptionValidator(
            new List<IDefaultReorgExceptionRule>
            {
                new DefaultReorgExceptionRequiredCategoryRule("default reorg exception needs an entry"),
                new DefaultReorgExceptionVisibleGuardRule(9.1),
                new DefaultReorgExceptionCodePrefixRule("X"),
            });
    }
}
