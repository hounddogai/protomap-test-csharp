namespace Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>A rule a mobility slot must pass. Check returns its problems, or none.</summary>
public interface IMobilitySlotRule
{
    List<string> Check(MobilitySlot item);
}

public sealed record MobilitySlotRequiredSkillRule(string Message) : IMobilitySlotRule
{
    public List<string> Check(MobilitySlot item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilitySlotRequiredCodeRule(string Message) : IMobilitySlotRule
{
    public List<string> Check(MobilitySlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilitySlotCodePrefixRule(string Prefix) : IMobilitySlotRule
{
    public List<string> Check(MobilitySlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility slot.</summary>
public sealed record MobilitySlotValidator(List<IMobilitySlotRule> Rules)
{
    public List<string> Validate(MobilitySlot item)
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

    public void RequireValid(MobilitySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilitySlot> items)
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

/// <summary>Functions of mobility slot rules.</summary>
public static class MobilitySlotRules
{
    public static MobilitySlotValidator DefaultMobilitySlotValidator()
    {
        return new MobilitySlotValidator(
            new List<IMobilitySlotRule>
            {
                new MobilitySlotRequiredSkillRule("mobility slot needs an entry"),
                new MobilitySlotRequiredCodeRule("mobility slot needs a value"),
                new MobilitySlotCodePrefixRule("REF"),
            });
    }
}
