namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>A rule a reorg window must pass. Check returns its problems, or none.</summary>
public interface IReorgWindowRule
{
    List<string> Check(ReorgWindow item);
}

public sealed record ReorgWindowRequiredCodeRule(string Message) : IReorgWindowRule
{
    public List<string> Check(ReorgWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgWindowRequiredUnitRule(string Message) : IReorgWindowRule
{
    public List<string> Check(ReorgWindow item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgWindowCodePrefixRule(string Prefix) : IReorgWindowRule
{
    public List<string> Check(ReorgWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg window.</summary>
public sealed record ReorgWindowValidator(List<IReorgWindowRule> Rules)
{
    public List<string> Validate(ReorgWindow item)
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

    public void RequireValid(ReorgWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgWindow> items)
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

/// <summary>Functions of reorg window rules.</summary>
public static class ReorgWindowRules
{
    public static ReorgWindowValidator DefaultReorgWindowValidator()
    {
        return new ReorgWindowValidator(
            new List<IReorgWindowRule>
            {
                new ReorgWindowRequiredCodeRule("reorg window needs this field"),
                new ReorgWindowRequiredUnitRule("reorg window needs this field"),
                new ReorgWindowCodePrefixRule("REF"),
            });
    }
}
