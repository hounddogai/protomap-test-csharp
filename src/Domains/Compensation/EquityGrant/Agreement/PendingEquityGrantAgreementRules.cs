namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>A rule a pending equity grant agreement must pass. Check returns its problems, or none.</summary>
public interface IPendingEquityGrantAgreementRule
{
    List<string> Check(PendingEquityGrantAgreement item);
}

public sealed record PendingEquityGrantAgreementLocationLengthRule(int MaxLength) : IPendingEquityGrantAgreementRule
{
    public List<string> Check(PendingEquityGrantAgreement item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingEquityGrantAgreementNotesLengthRule(int MaxLength) : IPendingEquityGrantAgreementRule
{
    public List<string> Check(PendingEquityGrantAgreement item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingEquityGrantAgreementRequiredLocationRule(string Message) : IPendingEquityGrantAgreementRule
{
    public List<string> Check(PendingEquityGrantAgreement item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending equity grant agreement.</summary>
public sealed record PendingEquityGrantAgreementValidator(List<IPendingEquityGrantAgreementRule> Rules)
{
    public List<string> Validate(PendingEquityGrantAgreement item)
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

    public void RequireValid(PendingEquityGrantAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingEquityGrantAgreement> items)
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

/// <summary>Functions of pending equity grant agreement rules.</summary>
public static class PendingEquityGrantAgreementRules
{
    public static PendingEquityGrantAgreementValidator DefaultPendingEquityGrantAgreementValidator()
    {
        return new PendingEquityGrantAgreementValidator(
            new List<IPendingEquityGrantAgreementRule>
            {
                new PendingEquityGrantAgreementLocationLengthRule(120),
                new PendingEquityGrantAgreementNotesLengthRule(3600),
                new PendingEquityGrantAgreementRequiredLocationRule(
                    "pending equity grant agreement needs this field"),
            });
    }
}
