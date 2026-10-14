namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient equity grant ledger must pass. Check returns its problems, or none.</summary>
public interface ILenientEquityGrantLedgerRule
{
    List<string> Check(LenientEquityGrantLedger item);
}

public sealed record LenientEquityGrantLedgerTitleLengthRule(int MaxLength) : ILenientEquityGrantLedgerRule
{
    public List<string> Check(LenientEquityGrantLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantLedgerCurrencyLengthRule(int MaxLength) : ILenientEquityGrantLedgerRule
{
    public List<string> Check(LenientEquityGrantLedger item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantLedgerAllowedCodeRule(List<string> Allowed) : ILenientEquityGrantLedgerRule
{
    public List<string> Check(LenientEquityGrantLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantLedgerNameLengthRule(int MaxLength) : ILenientEquityGrantLedgerRule
{
    public List<string> Check(LenientEquityGrantLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient equity grant ledger.</summary>
public sealed record LenientEquityGrantLedgerValidator(List<ILenientEquityGrantLedgerRule> Rules)
{
    public List<string> Validate(LenientEquityGrantLedger item)
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

    public void RequireValid(LenientEquityGrantLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientEquityGrantLedger> items)
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

/// <summary>Functions of lenient equity grant ledger rules.</summary>
public static class LenientEquityGrantLedgerRules
{
    public static LenientEquityGrantLedgerValidator DefaultLenientEquityGrantLedgerValidator()
    {
        return new LenientEquityGrantLedgerValidator(
            new List<ILenientEquityGrantLedgerRule>
            {
                new LenientEquityGrantLedgerTitleLengthRule(64),
                new LenientEquityGrantLedgerCurrencyLengthRule(1000),
                new LenientEquityGrantLedgerAllowedCodeRule(new List<string> { "default", "seasonal" }),
                new LenientEquityGrantLedgerNameLengthRule(64),
            });
    }
}
