namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback license seat reservation must pass. Check returns its problems, or none.</summary>
public interface IFallbackLicenseSeatReservationRule
{
    List<string> Check(FallbackLicenseSeatReservation item);
}

public sealed record FallbackLicenseSeatReservationRequiredTimezoneRule(string Message) : IFallbackLicenseSeatReservationRule
{
    public List<string> Check(FallbackLicenseSeatReservation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatReservationRetriesRangeRule(int Min, int Max) : IFallbackLicenseSeatReservationRule
{
    public List<string> Check(FallbackLicenseSeatReservation item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackLicenseSeatReservationRequiredCodeRule(string Message) : IFallbackLicenseSeatReservationRule
{
    public List<string> Check(FallbackLicenseSeatReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatReservationRequiredNameRule(string Message) : IFallbackLicenseSeatReservationRule
{
    public List<string> Check(FallbackLicenseSeatReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatReservationAllowedNameRule(List<string> Allowed) : IFallbackLicenseSeatReservationRule
{
    public List<string> Check(FallbackLicenseSeatReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback license seat reservation.</summary>
public sealed record FallbackLicenseSeatReservationValidator(List<IFallbackLicenseSeatReservationRule> Rules)
{
    public List<string> Validate(FallbackLicenseSeatReservation item)
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

    public void RequireValid(FallbackLicenseSeatReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLicenseSeatReservation> items)
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

/// <summary>Functions of fallback license seat reservation rules.</summary>
public static class FallbackLicenseSeatReservationRules
{
    public static FallbackLicenseSeatReservationValidator DefaultFallbackLicenseSeatReservationValidator()
    {
        return new FallbackLicenseSeatReservationValidator(
            new List<IFallbackLicenseSeatReservationRule>
            {
                new FallbackLicenseSeatReservationRequiredTimezoneRule(
                    "fallback license seat reservation needs a value"),
                new FallbackLicenseSeatReservationRetriesRangeRule(32, 80),
                new FallbackLicenseSeatReservationRequiredCodeRule(
                    "fallback license seat reservation needs an entry"),
                new FallbackLicenseSeatReservationRequiredNameRule(
                    "fallback license seat reservation needs this field"),
                new FallbackLicenseSeatReservationAllowedNameRule(new List<string> { "tiered", "legacy" }),
            });
    }
}
