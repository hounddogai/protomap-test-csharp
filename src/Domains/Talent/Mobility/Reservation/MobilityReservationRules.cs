namespace Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>A rule a mobility reservation must pass. Check returns its problems, or none.</summary>
public interface IMobilityReservationRule
{
    List<string> Check(MobilityReservation item);
}

public sealed record MobilityReservationRequiredGradeRule(string Message) : IMobilityReservationRule
{
    public List<string> Check(MobilityReservation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityReservationRequiredCountryRule(string Message) : IMobilityReservationRule
{
    public List<string> Check(MobilityReservation item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityReservationRequiredLabelRule(string Message) : IMobilityReservationRule
{
    public List<string> Check(MobilityReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility reservation.</summary>
public sealed record MobilityReservationValidator(List<IMobilityReservationRule> Rules)
{
    public List<string> Validate(MobilityReservation item)
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

    public void RequireValid(MobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityReservation> items)
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

/// <summary>Functions of mobility reservation rules.</summary>
public static class MobilityReservationRules
{
    public static MobilityReservationValidator DefaultMobilityReservationValidator()
    {
        return new MobilityReservationValidator(
            new List<IMobilityReservationRule>
            {
                new MobilityReservationRequiredGradeRule("mobility reservation needs an entry"),
                new MobilityReservationRequiredCountryRule("mobility reservation needs a value"),
                new MobilityReservationRequiredLabelRule("mobility reservation needs an entry"),
            });
    }
}
