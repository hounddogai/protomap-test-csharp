namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary scenario agreement must pass. Check returns its problems, or none.</summary>
public interface IPrimaryScenarioAgreementRule
{
    List<string> Check(PrimaryScenarioAgreement item);
}

public sealed record PrimaryScenarioAgreementRequiredLabelRule(string Message) : IPrimaryScenarioAgreementRule
{
    public List<string> Check(PrimaryScenarioAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioAgreementNameLengthRule(int MaxLength) : IPrimaryScenarioAgreementRule
{
    public List<string> Check(PrimaryScenarioAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioAgreementAllowedLabelRule(List<string> Allowed) : IPrimaryScenarioAgreementRule
{
    public List<string> Check(PrimaryScenarioAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioAgreementRequiredNameRule(string Message) : IPrimaryScenarioAgreementRule
{
    public List<string> Check(PrimaryScenarioAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary scenario agreement.</summary>
public sealed record PrimaryScenarioAgreementValidator(List<IPrimaryScenarioAgreementRule> Rules)
{
    public List<string> Validate(PrimaryScenarioAgreement item)
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

    public void RequireValid(PrimaryScenarioAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryScenarioAgreement> items)
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

/// <summary>Functions of primary scenario agreement rules.</summary>
public static class PrimaryScenarioAgreementRules
{
    public static PrimaryScenarioAgreementValidator DefaultPrimaryScenarioAgreementValidator()
    {
        return new PrimaryScenarioAgreementValidator(
            new List<IPrimaryScenarioAgreementRule>
            {
                new PrimaryScenarioAgreementRequiredLabelRule("primary scenario agreement needs a value"),
                new PrimaryScenarioAgreementNameLengthRule(64),
                new PrimaryScenarioAgreementAllowedLabelRule(new List<string> { "seasonal", "capped", "scheduled" }),
                new PrimaryScenarioAgreementRequiredNameRule("primary scenario agreement needs this field"),
            });
    }
}
