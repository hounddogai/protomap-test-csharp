namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>A rule a legacy license seat slot must pass. Check returns its problems, or none.</summary>
public interface ILegacyLicenseSeatSlotRule
{
    List<string> Check(LegacyLicenseSeatSlot item);
}

public sealed record LegacyLicenseSeatSlotRequiredCodeRule(string Message) : ILegacyLicenseSeatSlotRule
{
    public List<string> Check(LegacyLicenseSeatSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatSlotRequiredNameRule(string Message) : ILegacyLicenseSeatSlotRule
{
    public List<string> Check(LegacyLicenseSeatSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatSlotCodePrefixRule(string Prefix) : ILegacyLicenseSeatSlotRule
{
    public List<string> Check(LegacyLicenseSeatSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatSlotNameLengthRule(int MaxLength) : ILegacyLicenseSeatSlotRule
{
    public List<string> Check(LegacyLicenseSeatSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatSlotAttemptsRangeRule(int Min, int Max) : ILegacyLicenseSeatSlotRule
{
    public List<string> Check(LegacyLicenseSeatSlot item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy license seat slot.</summary>
public sealed record LegacyLicenseSeatSlotValidator(List<ILegacyLicenseSeatSlotRule> Rules)
{
    public List<string> Validate(LegacyLicenseSeatSlot item)
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

    public void RequireValid(LegacyLicenseSeatSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLicenseSeatSlot> items)
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

/// <summary>Functions of legacy license seat slot rules.</summary>
public static class LegacyLicenseSeatSlotRules
{
    public static LegacyLicenseSeatSlotValidator DefaultLegacyLicenseSeatSlotValidator()
    {
        return new LegacyLicenseSeatSlotValidator(
            new List<ILegacyLicenseSeatSlotRule>
            {
                new LegacyLicenseSeatSlotRequiredCodeRule("legacy license seat slot needs this field"),
                new LegacyLicenseSeatSlotRequiredNameRule("legacy license seat slot needs an entry"),
                new LegacyLicenseSeatSlotCodePrefixRule("X"),
                new LegacyLicenseSeatSlotNameLengthRule(255),
                new LegacyLicenseSeatSlotAttemptsRangeRule(120, 1000),
            });
    }
}
