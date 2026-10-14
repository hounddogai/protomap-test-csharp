namespace Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>A rule a fallback desk slot must pass. Check returns its problems, or none.</summary>
public interface IFallbackDeskSlotRule
{
    List<string> Check(FallbackDeskSlot item);
}

public sealed record FallbackDeskSlotRequiredTitleRule(string Message) : IFallbackDeskSlotRule
{
    public List<string> Check(FallbackDeskSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskSlotRequiredCodeRule(string Message) : IFallbackDeskSlotRule
{
    public List<string> Check(FallbackDeskSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackDeskSlotCodePrefixRule(string Prefix) : IFallbackDeskSlotRule
{
    public List<string> Check(FallbackDeskSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback desk slot.</summary>
public sealed record FallbackDeskSlotValidator(List<IFallbackDeskSlotRule> Rules)
{
    public List<string> Validate(FallbackDeskSlot item)
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

    public void RequireValid(FallbackDeskSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackDeskSlot> items)
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

/// <summary>Functions of fallback desk slot rules.</summary>
public static class FallbackDeskSlotRules
{
    public static FallbackDeskSlotValidator DefaultFallbackDeskSlotValidator()
    {
        return new FallbackDeskSlotValidator(
            new List<IFallbackDeskSlotRule>
            {
                new FallbackDeskSlotRequiredTitleRule("fallback desk slot needs a value"),
                new FallbackDeskSlotRequiredCodeRule("fallback desk slot needs an entry"),
                new FallbackDeskSlotCodePrefixRule("Q"),
            });
    }
}
