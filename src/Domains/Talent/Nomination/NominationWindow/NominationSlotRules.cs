namespace Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>A rule a nomination slot must pass. Check returns its problems, or none.</summary>
public interface INominationSlotRule
{
    List<string> Check(NominationSlot item);
}

public sealed record NominationSlotLeaveDaysRangeRule(int Min, int Max) : INominationSlotRule
{
    public List<string> Check(NominationSlot item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSlotCodePrefixRule(string Prefix) : INominationSlotRule
{
    public List<string> Check(NominationSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSlotTeamLengthRule(int MaxLength) : INominationSlotRule
{
    public List<string> Check(NominationSlot item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSlotMinimumRangeRule(double Min, double Max) : INominationSlotRule
{
    public List<string> Check(NominationSlot item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSlotCodeLengthRule(int MaxLength) : INominationSlotRule
{
    public List<string> Check(NominationSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination slot.</summary>
public sealed record NominationSlotValidator(List<INominationSlotRule> Rules)
{
    public List<string> Validate(NominationSlot item)
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

    public void RequireValid(NominationSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationSlot> items)
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

/// <summary>Functions of nomination slot rules.</summary>
public static class NominationSlotRules
{
    public static NominationSlotValidator DefaultNominationSlotValidator()
    {
        return new NominationSlotValidator(
            new List<INominationSlotRule>
            {
                new NominationSlotLeaveDaysRangeRule(80, 86400),
                new NominationSlotCodePrefixRule("Q"),
                new NominationSlotTeamLengthRule(255),
                new NominationSlotMinimumRangeRule(7.1, 1434.5),
                new NominationSlotCodeLengthRule(120),
            });
    }
}
