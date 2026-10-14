namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient span ledger must pass. Check returns its problems, or none.</summary>
public interface ILenientSpanLedgerRule
{
    List<string> Check(LenientSpanLedger item);
}

public sealed record LenientSpanLedgerUpdatedAtRangeRule(int Min, int Max) : ILenientSpanLedgerRule
{
    public List<string> Check(LenientSpanLedger item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanLedgerVisibleGuardRule(int Limit) : ILenientSpanLedgerRule
{
    public List<string> Check(LenientSpanLedger item)
    {
        if (item.Visible && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LenientSpanLedgerAllowedLabelRule(List<string> Allowed) : ILenientSpanLedgerRule
{
    public List<string> Check(LenientSpanLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient span ledger.</summary>
public sealed record LenientSpanLedgerValidator(List<ILenientSpanLedgerRule> Rules)
{
    public List<string> Validate(LenientSpanLedger item)
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

    public void RequireValid(LenientSpanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSpanLedger> items)
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

/// <summary>Functions of lenient span ledger rules.</summary>
public static class LenientSpanLedgerRules
{
    public static LenientSpanLedgerValidator DefaultLenientSpanLedgerValidator()
    {
        return new LenientSpanLedgerValidator(
            new List<ILenientSpanLedgerRule>
            {
                new LenientSpanLedgerUpdatedAtRangeRule(120, 3600),
                new LenientSpanLedgerVisibleGuardRule(64),
                new LenientSpanLedgerAllowedLabelRule(new List<string> { "tiered" }),
            });
    }
}
