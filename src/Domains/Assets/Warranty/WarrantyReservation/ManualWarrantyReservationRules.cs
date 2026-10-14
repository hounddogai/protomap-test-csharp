namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>A rule a manual warranty reservation must pass. Check returns its problems, or none.</summary>
public interface IManualWarrantyReservationRule
{
    List<string> Check(ManualWarrantyReservation item);
}

public sealed record ManualWarrantyReservationReviewerLengthRule(int MaxLength) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualWarrantyReservationRequiredLabelRule(string Message) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualWarrantyReservationVersionRangeRule(int Min, int Max) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualWarrantyReservationLabelLengthRule(int MaxLength) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualWarrantyReservationExpiresAtRangeRule(int Min, int Max) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualWarrantyReservationCodePrefixRule(string Prefix) : IManualWarrantyReservationRule
{
    public List<string> Check(ManualWarrantyReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual warranty reservation.</summary>
public sealed record ManualWarrantyReservationValidator(List<IManualWarrantyReservationRule> Rules)
{
    public List<string> Validate(ManualWarrantyReservation item)
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

    public void RequireValid(ManualWarrantyReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualWarrantyReservation> items)
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

/// <summary>Functions of manual warranty reservation rules.</summary>
public static class ManualWarrantyReservationRules
{
    public static ManualWarrantyReservationValidator DefaultManualWarrantyReservationValidator()
    {
        return new ManualWarrantyReservationValidator(
            new List<IManualWarrantyReservationRule>
            {
                new ManualWarrantyReservationReviewerLengthRule(1000),
                new ManualWarrantyReservationRequiredLabelRule("manual warranty reservation needs an entry"),
                new ManualWarrantyReservationVersionRangeRule(255, 86400),
                new ManualWarrantyReservationLabelLengthRule(120),
                new ManualWarrantyReservationExpiresAtRangeRule(32, 1000),
                new ManualWarrantyReservationCodePrefixRule("ACM"),
            });
    }
}
