namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>A rule a legacy desk entry must pass. Check returns its problems, or none.</summary>
public interface ILegacyDeskEntryRule
{
    List<string> Check(LegacyDeskEntry item);
}

public sealed record LegacyDeskEntryExternalRefLengthRule(int MaxLength) : ILegacyDeskEntryRule
{
    public List<string> Check(LegacyDeskEntry item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskEntryRequiredExternalRefRule(string Message) : ILegacyDeskEntryRule
{
    public List<string> Check(LegacyDeskEntry item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskEntryRequiredChannelRule(string Message) : ILegacyDeskEntryRule
{
    public List<string> Check(LegacyDeskEntry item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy desk entry.</summary>
public sealed record LegacyDeskEntryValidator(List<ILegacyDeskEntryRule> Rules)
{
    public List<string> Validate(LegacyDeskEntry item)
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

    public void RequireValid(LegacyDeskEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyDeskEntry> items)
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

/// <summary>Functions of legacy desk entry rules.</summary>
public static class LegacyDeskEntryRules
{
    public static LegacyDeskEntryValidator DefaultLegacyDeskEntryValidator()
    {
        return new LegacyDeskEntryValidator(
            new List<ILegacyDeskEntryRule>
            {
                new LegacyDeskEntryExternalRefLengthRule(1000),
                new LegacyDeskEntryRequiredExternalRefRule("legacy desk entry needs an entry"),
                new LegacyDeskEntryRequiredChannelRule("legacy desk entry needs a value"),
            });
    }
}
