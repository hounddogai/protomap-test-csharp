namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>A rule a weighted engagement agreement must pass. Check returns its problems, or none.</summary>
public interface IWeightedEngagementAgreementRule
{
    List<string> Check(WeightedEngagementAgreement item);
}

public sealed record WeightedEngagementAgreementContactEmailLengthRule(int MaxLength) : IWeightedEngagementAgreementRule
{
    public List<string> Check(WeightedEngagementAgreement item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEngagementAgreementRequiredContactEmailRule(string Message) : IWeightedEngagementAgreementRule
{
    public List<string> Check(WeightedEngagementAgreement item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEngagementAgreementLocationLengthRule(int MaxLength) : IWeightedEngagementAgreementRule
{
    public List<string> Check(WeightedEngagementAgreement item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted engagement agreement.</summary>
public sealed record WeightedEngagementAgreementValidator(List<IWeightedEngagementAgreementRule> Rules)
{
    public List<string> Validate(WeightedEngagementAgreement item)
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

    public void RequireValid(WeightedEngagementAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedEngagementAgreement> items)
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

/// <summary>Functions of weighted engagement agreement rules.</summary>
public static class WeightedEngagementAgreementRules
{
    public static WeightedEngagementAgreementValidator DefaultWeightedEngagementAgreementValidator()
    {
        return new WeightedEngagementAgreementValidator(
            new List<IWeightedEngagementAgreementRule>
            {
                new WeightedEngagementAgreementContactEmailLengthRule(80),
                new WeightedEngagementAgreementRequiredContactEmailRule(
                    "weighted engagement agreement needs this field"),
                new WeightedEngagementAgreementLocationLengthRule(86400),
            });
    }
}
