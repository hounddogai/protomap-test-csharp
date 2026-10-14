namespace Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>A rule a nomination entry must pass. Check returns its problems, or none.</summary>
public interface INominationEntryRule
{
    List<string> Check(NominationEntry item);
}

public sealed record NominationEntryNameLengthRule(int MaxLength) : INominationEntryRule
{
    public List<string> Check(NominationEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationEntryFloorValueRangeRule(double Min, double Max) : INominationEntryRule
{
    public List<string> Check(NominationEntry item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationEntryCodePrefixRule(string Prefix) : INominationEntryRule
{
    public List<string> Check(NominationEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationEntryApprovedGuardRule(double Limit) : INominationEntryRule
{
    public List<string> Check(NominationEntry item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination entry.</summary>
public sealed record NominationEntryValidator(List<INominationEntryRule> Rules)
{
    public List<string> Validate(NominationEntry item)
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

    public void RequireValid(NominationEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationEntry> items)
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

/// <summary>Functions of nomination entry rules.</summary>
public static class NominationEntryRules
{
    public static NominationEntryValidator DefaultNominationEntryValidator()
    {
        return new NominationEntryValidator(
            new List<INominationEntryRule>
            {
                new NominationEntryNameLengthRule(32),
                new NominationEntryFloorValueRangeRule(7.4, 4408.3),
                new NominationEntryCodePrefixRule("X"),
                new NominationEntryApprovedGuardRule(4.7),
            });
    }
}
