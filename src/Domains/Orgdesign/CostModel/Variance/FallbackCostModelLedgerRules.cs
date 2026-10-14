namespace Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>A rule a fallback cost model ledger must pass. Check returns its problems, or none.</summary>
public interface IFallbackCostModelLedgerRule
{
    List<string> Check(FallbackCostModelLedger item);
}

public sealed record FallbackCostModelLedgerNotesLengthRule(int MaxLength) : IFallbackCostModelLedgerRule
{
    public List<string> Check(FallbackCostModelLedger item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelLedgerRequiredContactEmailRule(string Message) : IFallbackCostModelLedgerRule
{
    public List<string> Check(FallbackCostModelLedger item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelLedgerCodePrefixRule(string Prefix) : IFallbackCostModelLedgerRule
{
    public List<string> Check(FallbackCostModelLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCostModelLedgerCodeLengthRule(int MaxLength) : IFallbackCostModelLedgerRule
{
    public List<string> Check(FallbackCostModelLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback cost model ledger.</summary>
public sealed record FallbackCostModelLedgerValidator(List<IFallbackCostModelLedgerRule> Rules)
{
    public List<string> Validate(FallbackCostModelLedger item)
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

    public void RequireValid(FallbackCostModelLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCostModelLedger> items)
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

/// <summary>Functions of fallback cost model ledger rules.</summary>
public static class FallbackCostModelLedgerRules
{
    public static FallbackCostModelLedgerValidator DefaultFallbackCostModelLedgerValidator()
    {
        return new FallbackCostModelLedgerValidator(
            new List<IFallbackCostModelLedgerRule>
            {
                new FallbackCostModelLedgerNotesLengthRule(255),
                new FallbackCostModelLedgerRequiredContactEmailRule("fallback cost model ledger needs this field"),
                new FallbackCostModelLedgerCodePrefixRule("OPS"),
                new FallbackCostModelLedgerCodeLengthRule(120),
            });
    }
}
