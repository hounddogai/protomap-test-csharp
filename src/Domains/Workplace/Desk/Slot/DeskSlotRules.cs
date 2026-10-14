namespace Acme.Workforce.Workplace.Desk.Slot;

/// <summary>A rule a desk slot must pass. Check returns its problems, or none.</summary>
public interface IDeskSlotRule
{
    List<string> Check(DeskSlot item);
}

public sealed record DeskSlotCodePrefixRule(string Prefix) : IDeskSlotRule
{
    public List<string> Check(DeskSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskSlotRequiredCountryRule(string Message) : IDeskSlotRule
{
    public List<string> Check(DeskSlot item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSlotCountryLengthRule(int MaxLength) : IDeskSlotRule
{
    public List<string> Check(DeskSlot item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk slot.</summary>
public sealed record DeskSlotValidator(List<IDeskSlotRule> Rules)
{
    public List<string> Validate(DeskSlot item)
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

    public void RequireValid(DeskSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskSlot> items)
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

/// <summary>Functions of desk slot rules.</summary>
public static class DeskSlotRules
{
    public static DeskSlotValidator DefaultDeskSlotValidator()
    {
        return new DeskSlotValidator(
            new List<IDeskSlotRule>
            {
                new DeskSlotCodePrefixRule("REF"),
                new DeskSlotRequiredCountryRule("desk slot needs an entry"),
                new DeskSlotCountryLengthRule(80),
            });
    }
}
