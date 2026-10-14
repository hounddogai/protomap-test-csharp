namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>A rule a legacy return plan ledger must pass. Check returns its problems, or none.</summary>
public interface ILegacyReturnPlanLedgerRule
{
    List<string> Check(LegacyReturnPlanLedger item);
}

public sealed record LegacyReturnPlanLedgerCodeLengthRule(int MaxLength) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyReturnPlanLedgerRequiredUnitRule(string Message) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyReturnPlanLedgerTenureMonthsRangeRule(int Min, int Max) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyReturnPlanLedgerRequiredCodeRule(string Message) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyReturnPlanLedgerLabelLengthRule(int MaxLength) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyReturnPlanLedgerGradeLengthRule(int MaxLength) : ILegacyReturnPlanLedgerRule
{
    public List<string> Check(LegacyReturnPlanLedger item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy return plan ledger.</summary>
public sealed record LegacyReturnPlanLedgerValidator(List<ILegacyReturnPlanLedgerRule> Rules)
{
    public List<string> Validate(LegacyReturnPlanLedger item)
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

    public void RequireValid(LegacyReturnPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyReturnPlanLedger> items)
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

/// <summary>Functions of legacy return plan ledger rules.</summary>
public static class LegacyReturnPlanLedgerRules
{
    public static LegacyReturnPlanLedgerValidator DefaultLegacyReturnPlanLedgerValidator()
    {
        return new LegacyReturnPlanLedgerValidator(
            new List<ILegacyReturnPlanLedgerRule>
            {
                new LegacyReturnPlanLedgerCodeLengthRule(32),
                new LegacyReturnPlanLedgerRequiredUnitRule("legacy return plan ledger needs an entry"),
                new LegacyReturnPlanLedgerTenureMonthsRangeRule(1000, 3600),
                new LegacyReturnPlanLedgerRequiredCodeRule("legacy return plan ledger needs an entry"),
                new LegacyReturnPlanLedgerLabelLengthRule(16),
                new LegacyReturnPlanLedgerGradeLengthRule(32),
            });
    }
}
