namespace Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>A rule a regional training due reservation must pass. Check returns its problems, or none.</summary>
public interface IRegionalTrainingDueReservationRule
{
    List<string> Check(RegionalTrainingDueReservation item);
}

public sealed record RegionalTrainingDueReservationCodePrefixRule(string Prefix) : IRegionalTrainingDueReservationRule
{
    public List<string> Check(RegionalTrainingDueReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueReservationRequiredCodeRule(string Message) : IRegionalTrainingDueReservationRule
{
    public List<string> Check(RegionalTrainingDueReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueReservationCodeLengthRule(int MaxLength) : IRegionalTrainingDueReservationRule
{
    public List<string> Check(RegionalTrainingDueReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueReservationRequiredCategoryRule(string Message) : IRegionalTrainingDueReservationRule
{
    public List<string> Check(RegionalTrainingDueReservation item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional training due reservation.</summary>
public sealed record RegionalTrainingDueReservationValidator(List<IRegionalTrainingDueReservationRule> Rules)
{
    public List<string> Validate(RegionalTrainingDueReservation item)
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

    public void RequireValid(RegionalTrainingDueReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTrainingDueReservation> items)
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

/// <summary>Functions of regional training due reservation rules.</summary>
public static class RegionalTrainingDueReservationRules
{
    public static RegionalTrainingDueReservationValidator DefaultRegionalTrainingDueReservationValidator()
    {
        return new RegionalTrainingDueReservationValidator(
            new List<IRegionalTrainingDueReservationRule>
            {
                new RegionalTrainingDueReservationCodePrefixRule("Q"),
                new RegionalTrainingDueReservationRequiredCodeRule(
                    "regional training due reservation needs this field"),
                new RegionalTrainingDueReservationCodeLengthRule(16),
                new RegionalTrainingDueReservationRequiredCategoryRule(
                    "regional training due reservation needs an entry"),
            });
    }
}
