namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>A rule a tiered license seat reservation must pass. Check returns its problems, or none.</summary>
public interface ITieredLicenseSeatReservationRule
{
    List<string> Check(TieredLicenseSeatReservation item);
}

public sealed record TieredLicenseSeatReservationLabelLengthRule(int MaxLength) : ITieredLicenseSeatReservationRule
{
    public List<string> Check(TieredLicenseSeatReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatReservationRequiredCategoryRule(string Message) : ITieredLicenseSeatReservationRule
{
    public List<string> Check(TieredLicenseSeatReservation item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatReservationRequiredManagerIdRule(string Message) : ITieredLicenseSeatReservationRule
{
    public List<string> Check(TieredLicenseSeatReservation item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered license seat reservation.</summary>
public sealed record TieredLicenseSeatReservationValidator(List<ITieredLicenseSeatReservationRule> Rules)
{
    public List<string> Validate(TieredLicenseSeatReservation item)
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

    public void RequireValid(TieredLicenseSeatReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLicenseSeatReservation> items)
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

/// <summary>Functions of tiered license seat reservation rules.</summary>
public static class TieredLicenseSeatReservationRules
{
    public static TieredLicenseSeatReservationValidator DefaultTieredLicenseSeatReservationValidator()
    {
        return new TieredLicenseSeatReservationValidator(
            new List<ITieredLicenseSeatReservationRule>
            {
                new TieredLicenseSeatReservationLabelLengthRule(86400),
                new TieredLicenseSeatReservationRequiredCategoryRule("tiered license seat reservation needs a value"),
                new TieredLicenseSeatReservationRequiredManagerIdRule(
                    "tiered license seat reservation needs an entry"),
            });
    }
}
