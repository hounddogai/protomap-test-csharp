namespace Acme.Workforce.Talent.Nomination.Rule;

/// <summary>A rule a nomination reservation must pass. Check returns its problems, or none.</summary>
public interface INominationReservationRule
{
    List<string> Check(NominationReservation item);
}

public sealed record NominationReservationRequiredCodeRule(string Message) : INominationReservationRule
{
    public List<string> Check(NominationReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationReservationLocationLengthRule(int MaxLength) : INominationReservationRule
{
    public List<string> Check(NominationReservation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationReservationCodePrefixRule(string Prefix) : INominationReservationRule
{
    public List<string> Check(NominationReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination reservation.</summary>
public sealed record NominationReservationValidator(List<INominationReservationRule> Rules)
{
    public List<string> Validate(NominationReservation item)
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

    public void RequireValid(NominationReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationReservation> items)
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

/// <summary>Functions of nomination reservation rules.</summary>
public static class NominationReservationRules
{
    public static NominationReservationValidator DefaultNominationReservationValidator()
    {
        return new NominationReservationValidator(
            new List<INominationReservationRule>
            {
                new NominationReservationRequiredCodeRule("nomination reservation needs this field"),
                new NominationReservationLocationLengthRule(86400),
                new NominationReservationCodePrefixRule("X"),
            });
    }
}
