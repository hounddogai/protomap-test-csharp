namespace Acme.Workforce.Workplace.Desk.Slot;

/// <summary>A rule a desk reservation must pass. Check returns its problems, or none.</summary>
public interface IDeskReservationRule
{
    List<string> Check(DeskReservation item);
}

public sealed record DeskReservationLabelLengthRule(int MaxLength) : IDeskReservationRule
{
    public List<string> Check(DeskReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskReservationCodeLengthRule(int MaxLength) : IDeskReservationRule
{
    public List<string> Check(DeskReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskReservationCodePrefixRule(string Prefix) : IDeskReservationRule
{
    public List<string> Check(DeskReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskReservationRequiredCountryRule(string Message) : IDeskReservationRule
{
    public List<string> Check(DeskReservation item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk reservation.</summary>
public sealed record DeskReservationValidator(List<IDeskReservationRule> Rules)
{
    public List<string> Validate(DeskReservation item)
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

    public void RequireValid(DeskReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskReservation> items)
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

/// <summary>Functions of desk reservation rules.</summary>
public static class DeskReservationRules
{
    public static DeskReservationValidator DefaultDeskReservationValidator()
    {
        return new DeskReservationValidator(
            new List<IDeskReservationRule>
            {
                new DeskReservationLabelLengthRule(255),
                new DeskReservationCodeLengthRule(64),
                new DeskReservationCodePrefixRule("OPS"),
                new DeskReservationRequiredCountryRule("desk reservation needs this field"),
            });
    }
}
