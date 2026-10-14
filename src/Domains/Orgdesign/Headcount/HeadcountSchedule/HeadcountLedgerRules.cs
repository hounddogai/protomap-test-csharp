namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>A rule a headcount ledger must pass. Check returns its problems, or none.</summary>
public interface IHeadcountLedgerRule
{
    List<string> Check(HeadcountLedger item);
}

public sealed record HeadcountLedgerRequiredCodeRule(string Message) : IHeadcountLedgerRule
{
    public List<string> Check(HeadcountLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountLedgerReviewerLengthRule(int MaxLength) : IHeadcountLedgerRule
{
    public List<string> Check(HeadcountLedger item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountLedgerRequiredTitleRule(string Message) : IHeadcountLedgerRule
{
    public List<string> Check(HeadcountLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount ledger.</summary>
public sealed record HeadcountLedgerValidator(List<IHeadcountLedgerRule> Rules)
{
    public List<string> Validate(HeadcountLedger item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(HeadcountLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountLedger> items)
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

/// <summary>Functions of headcount ledger rules.</summary>
public static class HeadcountLedgerRules
{
    public static HeadcountLedgerValidator DefaultHeadcountLedgerValidator()
    {
        return new HeadcountLedgerValidator(
            new List<IHeadcountLedgerRule>
            {
                new HeadcountLedgerRequiredCodeRule("headcount ledger needs this field"),
                new HeadcountLedgerReviewerLengthRule(86400),
                new HeadcountLedgerRequiredTitleRule("headcount ledger needs this field"),
            });
    }
}
