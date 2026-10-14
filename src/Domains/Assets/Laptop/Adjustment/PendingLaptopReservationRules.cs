namespace Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>A rule a pending laptop reservation must pass. Check returns its problems, or none.</summary>
public interface IPendingLaptopReservationRule
{
    List<string> Check(PendingLaptopReservation item);
}

public sealed record PendingLaptopReservationCodeLengthRule(int MaxLength) : IPendingLaptopReservationRule
{
    public List<string> Check(PendingLaptopReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopReservationRequiredTitleRule(string Message) : IPendingLaptopReservationRule
{
    public List<string> Check(PendingLaptopReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopReservationCodePrefixRule(string Prefix) : IPendingLaptopReservationRule
{
    public List<string> Check(PendingLaptopReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending laptop reservation.</summary>
public sealed record PendingLaptopReservationValidator(List<IPendingLaptopReservationRule> Rules)
{
    public List<string> Validate(PendingLaptopReservation item)
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

    public void RequireValid(PendingLaptopReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLaptopReservation> items)
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

/// <summary>Functions of pending laptop reservation rules.</summary>
public static class PendingLaptopReservationRules
{
    public static PendingLaptopReservationValidator DefaultPendingLaptopReservationValidator()
    {
        return new PendingLaptopReservationValidator(
            new List<IPendingLaptopReservationRule>
            {
                new PendingLaptopReservationCodeLengthRule(16),
                new PendingLaptopReservationRequiredTitleRule("pending laptop reservation needs an entry"),
                new PendingLaptopReservationCodePrefixRule("X"),
            });
    }
}
