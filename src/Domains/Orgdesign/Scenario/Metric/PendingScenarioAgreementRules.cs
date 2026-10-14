namespace Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>A rule a pending scenario agreement must pass. Check returns its problems, or none.</summary>
public interface IPendingScenarioAgreementRule
{
    List<string> Check(PendingScenarioAgreement item);
}

public sealed record PendingScenarioAgreementCapacityRangeRule(int Min, int Max) : IPendingScenarioAgreementRule
{
    public List<string> Check(PendingScenarioAgreement item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingScenarioAgreementCodeLengthRule(int MaxLength) : IPendingScenarioAgreementRule
{
    public List<string> Check(PendingScenarioAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingScenarioAgreementRequiredExternalRefRule(string Message) : IPendingScenarioAgreementRule
{
    public List<string> Check(PendingScenarioAgreement item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending scenario agreement.</summary>
public sealed record PendingScenarioAgreementValidator(List<IPendingScenarioAgreementRule> Rules)
{
    public List<string> Validate(PendingScenarioAgreement item)
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

    public void RequireValid(PendingScenarioAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingScenarioAgreement> items)
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

/// <summary>Functions of pending scenario agreement rules.</summary>
public static class PendingScenarioAgreementRules
{
    public static PendingScenarioAgreementValidator DefaultPendingScenarioAgreementValidator()
    {
        return new PendingScenarioAgreementValidator(
            new List<IPendingScenarioAgreementRule>
            {
                new PendingScenarioAgreementCapacityRangeRule(16, 3600),
                new PendingScenarioAgreementCodeLengthRule(1000),
                new PendingScenarioAgreementRequiredExternalRefRule("pending scenario agreement needs an entry"),
            });
    }
}
