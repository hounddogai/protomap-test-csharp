namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped pay run ledger must pass. Check returns its problems, or none.</summary>
public interface ICappedPayRunLedgerRule
{
    List<string> Check(CappedPayRunLedger item);
}

public sealed record CappedPayRunLedgerBatchSizeRangeRule(int Min, int Max) : ICappedPayRunLedgerRule
{
    public List<string> Check(CappedPayRunLedger item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunLedgerRequiredChannelRule(string Message) : ICappedPayRunLedgerRule
{
    public List<string> Check(CappedPayRunLedger item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunLedgerAllowedCodeRule(List<string> Allowed) : ICappedPayRunLedgerRule
{
    public List<string> Check(CappedPayRunLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunLedgerCodePrefixRule(string Prefix) : ICappedPayRunLedgerRule
{
    public List<string> Check(CappedPayRunLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunLedgerRequiredCodeRule(string Message) : ICappedPayRunLedgerRule
{
    public List<string> Check(CappedPayRunLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay run ledger.</summary>
public sealed record CappedPayRunLedgerValidator(List<ICappedPayRunLedgerRule> Rules)
{
    public List<string> Validate(CappedPayRunLedger item)
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

    public void RequireValid(CappedPayRunLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayRunLedger> items)
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

/// <summary>Functions of capped pay run ledger rules.</summary>
public static class CappedPayRunLedgerRules
{
    public static CappedPayRunLedgerValidator DefaultCappedPayRunLedgerValidator()
    {
        return new CappedPayRunLedgerValidator(
            new List<ICappedPayRunLedgerRule>
            {
                new CappedPayRunLedgerBatchSizeRangeRule(3600, 86400),
                new CappedPayRunLedgerRequiredChannelRule("capped pay run ledger needs a value"),
                new CappedPayRunLedgerAllowedCodeRule(new List<string> { "legacy", "strict" }),
                new CappedPayRunLedgerCodePrefixRule("REF"),
                new CappedPayRunLedgerRequiredCodeRule("capped pay run ledger needs this field"),
            });
    }
}
