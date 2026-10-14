namespace Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>A rule a attestation plan must pass. Check returns its problems, or none.</summary>
public interface IAttestationPlanRule
{
    List<string> Check(AttestationPlan item);
}

public sealed record AttestationPlanLeaveDaysRangeRule(int Min, int Max) : IAttestationPlanRule
{
    public List<string> Check(AttestationPlan item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationPlanTitleLengthRule(int MaxLength) : IAttestationPlanRule
{
    public List<string> Check(AttestationPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationPlanRequiredCurrencyRule(string Message) : IAttestationPlanRule
{
    public List<string> Check(AttestationPlan item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation plan.</summary>
public sealed record AttestationPlanValidator(List<IAttestationPlanRule> Rules)
{
    public List<string> Validate(AttestationPlan item)
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

    public void RequireValid(AttestationPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationPlan> items)
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

/// <summary>Functions of attestation plan rules.</summary>
public static class AttestationPlanRules
{
    public static AttestationPlanValidator DefaultAttestationPlanValidator()
    {
        return new AttestationPlanValidator(
            new List<IAttestationPlanRule>
            {
                new AttestationPlanLeaveDaysRangeRule(16, 120),
                new AttestationPlanTitleLengthRule(1000),
                new AttestationPlanRequiredCurrencyRule("attestation plan needs an entry"),
            });
    }
}
