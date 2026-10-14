namespace Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>A rule a lenient visitor reservation must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorReservationRule
{
    List<string> Check(LenientVisitorReservation item);
}

public sealed record LenientVisitorReservationCodePrefixRule(string Prefix) : ILenientVisitorReservationRule
{
    public List<string> Check(LenientVisitorReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorReservationRequiredNameRule(string Message) : ILenientVisitorReservationRule
{
    public List<string> Check(LenientVisitorReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorReservationLocationLengthRule(int MaxLength) : ILenientVisitorReservationRule
{
    public List<string> Check(LenientVisitorReservation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorReservationNameLengthRule(int MaxLength) : ILenientVisitorReservationRule
{
    public List<string> Check(LenientVisitorReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorReservationRequiredCodeRule(string Message) : ILenientVisitorReservationRule
{
    public List<string> Check(LenientVisitorReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor reservation.</summary>
public sealed record LenientVisitorReservationValidator(List<ILenientVisitorReservationRule> Rules)
{
    public List<string> Validate(LenientVisitorReservation item)
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

    public void RequireValid(LenientVisitorReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorReservation> items)
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

/// <summary>Functions of lenient visitor reservation rules.</summary>
public static class LenientVisitorReservationRules
{
    public static LenientVisitorReservationValidator DefaultLenientVisitorReservationValidator()
    {
        return new LenientVisitorReservationValidator(
            new List<ILenientVisitorReservationRule>
            {
                new LenientVisitorReservationCodePrefixRule("ACM"),
                new LenientVisitorReservationRequiredNameRule("lenient visitor reservation needs an entry"),
                new LenientVisitorReservationLocationLengthRule(255),
                new LenientVisitorReservationNameLengthRule(86400),
                new LenientVisitorReservationRequiredCodeRule("lenient visitor reservation needs an entry"),
            });
    }
}
