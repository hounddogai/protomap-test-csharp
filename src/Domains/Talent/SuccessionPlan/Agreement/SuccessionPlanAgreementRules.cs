namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>A rule a succession plan agreement must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanAgreementRule
{
    List<string> Check(SuccessionPlanAgreement item);
}

public sealed record SuccessionPlanAgreementRevisionRangeRule(int Min, int Max) : ISuccessionPlanAgreementRule
{
    public List<string> Check(SuccessionPlanAgreement item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAgreementRequiredNameRule(string Message) : ISuccessionPlanAgreementRule
{
    public List<string> Check(SuccessionPlanAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAgreementRequiredCountryRule(string Message) : ISuccessionPlanAgreementRule
{
    public List<string> Check(SuccessionPlanAgreement item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan agreement.</summary>
public sealed record SuccessionPlanAgreementValidator(List<ISuccessionPlanAgreementRule> Rules)
{
    public List<string> Validate(SuccessionPlanAgreement item)
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

    public void RequireValid(SuccessionPlanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanAgreement> items)
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

/// <summary>Functions of succession plan agreement rules.</summary>
public static class SuccessionPlanAgreementRules
{
    public static SuccessionPlanAgreementValidator DefaultSuccessionPlanAgreementValidator()
    {
        return new SuccessionPlanAgreementValidator(
            new List<ISuccessionPlanAgreementRule>
            {
                new SuccessionPlanAgreementRevisionRangeRule(64, 86400),
                new SuccessionPlanAgreementRequiredNameRule("succession plan agreement needs a value"),
                new SuccessionPlanAgreementRequiredCountryRule("succession plan agreement needs this field"),
            });
    }
}
