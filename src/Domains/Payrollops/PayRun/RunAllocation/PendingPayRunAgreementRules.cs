namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pay run agreement must pass. Check returns its problems, or none.</summary>
public interface IPendingPayRunAgreementRule
{
    List<string> Check(PendingPayRunAgreement item);
}

public sealed record PendingPayRunAgreementAllowedCodeRule(List<string> Allowed) : IPendingPayRunAgreementRule
{
    public List<string> Check(PendingPayRunAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAgreementRequiredCodeRule(string Message) : IPendingPayRunAgreementRule
{
    public List<string> Check(PendingPayRunAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAgreementAmountRangeRule(double Min, double Max) : IPendingPayRunAgreementRule
{
    public List<string> Check(PendingPayRunAgreement item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending pay run agreement.</summary>
public sealed record PendingPayRunAgreementValidator(List<IPendingPayRunAgreementRule> Rules)
{
    public List<string> Validate(PendingPayRunAgreement item)
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

    public void RequireValid(PendingPayRunAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayRunAgreement> items)
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

/// <summary>Functions of pending pay run agreement rules.</summary>
public static class PendingPayRunAgreementRules
{
    public static PendingPayRunAgreementValidator DefaultPendingPayRunAgreementValidator()
    {
        return new PendingPayRunAgreementValidator(
            new List<IPendingPayRunAgreementRule>
            {
                new PendingPayRunAgreementAllowedCodeRule(new List<string>()),
                new PendingPayRunAgreementRequiredCodeRule("pending pay run agreement needs a value"),
                new PendingPayRunAgreementAmountRangeRule(7.3, 1150.1),
            });
    }
}
