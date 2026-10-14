namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal accrual ledger must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccrualLedgerRule
{
    List<string> Check(SeasonalAccrualLedger item);
}

public sealed record SeasonalAccrualLedgerRequiredContactEmailRule(string Message) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualLedgerAllowedContactEmailRule(List<string> Allowed) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualLedgerCodeLengthRule(int MaxLength) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualLedgerRatioRangeRule(double Min, double Max) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualLedgerContactEmailLengthRule(int MaxLength) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualLedgerQuantityRangeRule(int Min, int Max) : ISeasonalAccrualLedgerRule
{
    public List<string> Check(SeasonalAccrualLedger item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal accrual ledger.</summary>
public sealed record SeasonalAccrualLedgerValidator(List<ISeasonalAccrualLedgerRule> Rules)
{
    public List<string> Validate(SeasonalAccrualLedger item)
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

    public void RequireValid(SeasonalAccrualLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccrualLedger> items)
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

/// <summary>Functions of seasonal accrual ledger rules.</summary>
public static class SeasonalAccrualLedgerRules
{
    public static SeasonalAccrualLedgerValidator DefaultSeasonalAccrualLedgerValidator()
    {
        return new SeasonalAccrualLedgerValidator(
            new List<ISeasonalAccrualLedgerRule>
            {
                new SeasonalAccrualLedgerRequiredContactEmailRule("seasonal accrual ledger needs a value"),
                new SeasonalAccrualLedgerAllowedContactEmailRule(new List<string> { "adaptive", "capped", "capped" }),
                new SeasonalAccrualLedgerCodeLengthRule(255),
                new SeasonalAccrualLedgerRatioRangeRule(6.1, 770.6),
                new SeasonalAccrualLedgerContactEmailLengthRule(86400),
                new SeasonalAccrualLedgerQuantityRangeRule(64, 255),
            });
    }
}
