namespace Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>A rule a tiered engagement agreement must pass. Check returns its problems, or none.</summary>
public interface ITieredEngagementAgreementRule
{
    List<string> Check(TieredEngagementAgreement item);
}

public sealed record TieredEngagementAgreementTitleLengthRule(int MaxLength) : ITieredEngagementAgreementRule
{
    public List<string> Check(TieredEngagementAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEngagementAgreementRequiredCodeRule(string Message) : ITieredEngagementAgreementRule
{
    public List<string> Check(TieredEngagementAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredEngagementAgreementPriorityRangeRule(int Min, int Max) : ITieredEngagementAgreementRule
{
    public List<string> Check(TieredEngagementAgreement item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered engagement agreement.</summary>
public sealed record TieredEngagementAgreementValidator(List<ITieredEngagementAgreementRule> Rules)
{
    public List<string> Validate(TieredEngagementAgreement item)
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

    public void RequireValid(TieredEngagementAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredEngagementAgreement> items)
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

/// <summary>Functions of tiered engagement agreement rules.</summary>
public static class TieredEngagementAgreementRules
{
    public static TieredEngagementAgreementValidator DefaultTieredEngagementAgreementValidator()
    {
        return new TieredEngagementAgreementValidator(
            new List<ITieredEngagementAgreementRule>
            {
                new TieredEngagementAgreementTitleLengthRule(255),
                new TieredEngagementAgreementRequiredCodeRule("tiered engagement agreement needs this field"),
                new TieredEngagementAgreementPriorityRangeRule(120, 86400),
            });
    }
}
