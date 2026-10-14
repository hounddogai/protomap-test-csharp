namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>A rule a primary equity grant override must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEquityGrantOverrideRule
{
    List<string> Check(PrimaryEquityGrantOverride item);
}

public sealed record PrimaryEquityGrantOverrideTeamLengthRule(int MaxLength) : IPrimaryEquityGrantOverrideRule
{
    public List<string> Check(PrimaryEquityGrantOverride item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantOverrideActiveGuardRule(double Limit) : IPrimaryEquityGrantOverrideRule
{
    public List<string> Check(PrimaryEquityGrantOverride item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantOverrideCodePrefixRule(string Prefix) : IPrimaryEquityGrantOverrideRule
{
    public List<string> Check(PrimaryEquityGrantOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantOverrideRequiredCodeRule(string Message) : IPrimaryEquityGrantOverrideRule
{
    public List<string> Check(PrimaryEquityGrantOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary equity grant override.</summary>
public sealed record PrimaryEquityGrantOverrideValidator(List<IPrimaryEquityGrantOverrideRule> Rules)
{
    public List<string> Validate(PrimaryEquityGrantOverride item)
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

    public void RequireValid(PrimaryEquityGrantOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEquityGrantOverride> items)
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

/// <summary>Functions of primary equity grant override rules.</summary>
public static class PrimaryEquityGrantOverrideRules
{
    public static PrimaryEquityGrantOverrideValidator DefaultPrimaryEquityGrantOverrideValidator()
    {
        return new PrimaryEquityGrantOverrideValidator(
            new List<IPrimaryEquityGrantOverrideRule>
            {
                new PrimaryEquityGrantOverrideTeamLengthRule(255),
                new PrimaryEquityGrantOverrideActiveGuardRule(6.2),
                new PrimaryEquityGrantOverrideCodePrefixRule("REF"),
                new PrimaryEquityGrantOverrideRequiredCodeRule("primary equity grant override needs a value"),
            });
    }
}
