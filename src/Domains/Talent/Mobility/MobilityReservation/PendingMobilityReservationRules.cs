namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending mobility reservation must pass. Check returns its problems, or none.</summary>
public interface IPendingMobilityReservationRule
{
    List<string> Check(PendingMobilityReservation item);
}

public sealed record PendingMobilityReservationThresholdRangeRule(double Min, double Max) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingMobilityReservationAllowedTitleRule(List<string> Allowed) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PendingMobilityReservationCodeLengthRule(int MaxLength) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingMobilityReservationRequiredExternalRefRule(string Message) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingMobilityReservationTitleLengthRule(int MaxLength) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingMobilityReservationExternalRefLengthRule(int MaxLength) : IPendingMobilityReservationRule
{
    public List<string> Check(PendingMobilityReservation item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending mobility reservation.</summary>
public sealed record PendingMobilityReservationValidator(List<IPendingMobilityReservationRule> Rules)
{
    public List<string> Validate(PendingMobilityReservation item)
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

    public void RequireValid(PendingMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingMobilityReservation> items)
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

/// <summary>Functions of pending mobility reservation rules.</summary>
public static class PendingMobilityReservationRules
{
    public static PendingMobilityReservationValidator DefaultPendingMobilityReservationValidator()
    {
        return new PendingMobilityReservationValidator(
            new List<IPendingMobilityReservationRule>
            {
                new PendingMobilityReservationThresholdRangeRule(2.5, 2390.2),
                new PendingMobilityReservationAllowedTitleRule(new List<string> { "regional" }),
                new PendingMobilityReservationCodeLengthRule(1000),
                new PendingMobilityReservationRequiredExternalRefRule("pending mobility reservation needs a value"),
                new PendingMobilityReservationTitleLengthRule(86400),
                new PendingMobilityReservationExternalRefLengthRule(255),
            });
    }
}
