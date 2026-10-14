namespace Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>A rule a manual occupancy agreement must pass. Check returns its problems, or none.</summary>
public interface IManualOccupancyAgreementRule
{
    List<string> Check(ManualOccupancyAgreement item);
}

public sealed record ManualOccupancyAgreementTitleLengthRule(int MaxLength) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyAgreementCodeLengthRule(int MaxLength) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyAgreementRequiredTitleRule(string Message) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyAgreementRequiredCodeRule(string Message) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyAgreementCodePrefixRule(string Prefix) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualOccupancyAgreementGraceDaysRangeRule(int Min, int Max) : IManualOccupancyAgreementRule
{
    public List<string> Check(ManualOccupancyAgreement item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual occupancy agreement.</summary>
public sealed record ManualOccupancyAgreementValidator(List<IManualOccupancyAgreementRule> Rules)
{
    public List<string> Validate(ManualOccupancyAgreement item)
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

    public void RequireValid(ManualOccupancyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualOccupancyAgreement> items)
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

/// <summary>Functions of manual occupancy agreement rules.</summary>
public static class ManualOccupancyAgreementRules
{
    public static ManualOccupancyAgreementValidator DefaultManualOccupancyAgreementValidator()
    {
        return new ManualOccupancyAgreementValidator(
            new List<IManualOccupancyAgreementRule>
            {
                new ManualOccupancyAgreementTitleLengthRule(80),
                new ManualOccupancyAgreementCodeLengthRule(3600),
                new ManualOccupancyAgreementRequiredTitleRule("manual occupancy agreement needs a value"),
                new ManualOccupancyAgreementRequiredCodeRule("manual occupancy agreement needs this field"),
                new ManualOccupancyAgreementCodePrefixRule("Q"),
                new ManualOccupancyAgreementGraceDaysRangeRule(255, 86400),
            });
    }
}
