namespace Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>A rule a primary headcount ledger must pass. Check returns its problems, or none.</summary>
public interface IPrimaryHeadcountLedgerRule
{
    List<string> Check(PrimaryHeadcountLedger item);
}

public sealed record PrimaryHeadcountLedgerQuantityRangeRule(int Min, int Max) : IPrimaryHeadcountLedgerRule
{
    public List<string> Check(PrimaryHeadcountLedger item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountLedgerReviewerLengthRule(int MaxLength) : IPrimaryHeadcountLedgerRule
{
    public List<string> Check(PrimaryHeadcountLedger item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountLedgerRequiredReviewerRule(string Message) : IPrimaryHeadcountLedgerRule
{
    public List<string> Check(PrimaryHeadcountLedger item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountLedgerTimezoneLengthRule(int MaxLength) : IPrimaryHeadcountLedgerRule
{
    public List<string> Check(PrimaryHeadcountLedger item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountLedgerCodeLengthRule(int MaxLength) : IPrimaryHeadcountLedgerRule
{
    public List<string> Check(PrimaryHeadcountLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary headcount ledger.</summary>
public sealed record PrimaryHeadcountLedgerValidator(List<IPrimaryHeadcountLedgerRule> Rules)
{
    public List<string> Validate(PrimaryHeadcountLedger item)
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

    public void RequireValid(PrimaryHeadcountLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryHeadcountLedger> items)
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

/// <summary>Functions of primary headcount ledger rules.</summary>
public static class PrimaryHeadcountLedgerRules
{
    public static PrimaryHeadcountLedgerValidator DefaultPrimaryHeadcountLedgerValidator()
    {
        return new PrimaryHeadcountLedgerValidator(
            new List<IPrimaryHeadcountLedgerRule>
            {
                new PrimaryHeadcountLedgerQuantityRangeRule(64, 86400),
                new PrimaryHeadcountLedgerReviewerLengthRule(1000),
                new PrimaryHeadcountLedgerRequiredReviewerRule("primary headcount ledger needs this field"),
                new PrimaryHeadcountLedgerTimezoneLengthRule(64),
                new PrimaryHeadcountLedgerCodeLengthRule(3600),
            });
    }
}
