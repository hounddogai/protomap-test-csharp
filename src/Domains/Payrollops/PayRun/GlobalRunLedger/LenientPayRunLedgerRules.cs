namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient pay run ledger must pass. Check returns its problems, or none.</summary>
public interface ILenientPayRunLedgerRule
{
    List<string> Check(LenientPayRunLedger item);
}

public sealed record LenientPayRunLedgerDescriptionLengthRule(int MaxLength) : ILenientPayRunLedgerRule
{
    public List<string> Check(LenientPayRunLedger item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunLedgerCodeLengthRule(int MaxLength) : ILenientPayRunLedgerRule
{
    public List<string> Check(LenientPayRunLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunLedgerAllowedCodeRule(List<string> Allowed) : ILenientPayRunLedgerRule
{
    public List<string> Check(LenientPayRunLedger item)
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

/// <summary>Runs every rule of a lenient pay run ledger.</summary>
public sealed record LenientPayRunLedgerValidator(List<ILenientPayRunLedgerRule> Rules)
{
    public List<string> Validate(LenientPayRunLedger item)
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

    public void RequireValid(LenientPayRunLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayRunLedger> items)
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

/// <summary>Functions of lenient pay run ledger rules.</summary>
public static class LenientPayRunLedgerRules
{
    public static LenientPayRunLedgerValidator DefaultLenientPayRunLedgerValidator()
    {
        return new LenientPayRunLedgerValidator(
            new List<ILenientPayRunLedgerRule>
            {
                new LenientPayRunLedgerDescriptionLengthRule(86400),
                new LenientPayRunLedgerCodeLengthRule(64),
                new LenientPayRunLedgerAllowedCodeRule(new List<string>()),
            });
    }
}
