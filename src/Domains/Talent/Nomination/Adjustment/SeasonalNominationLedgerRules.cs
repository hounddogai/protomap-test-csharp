namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal nomination ledger must pass. Check returns its problems, or none.</summary>
public interface ISeasonalNominationLedgerRule
{
    List<string> Check(SeasonalNominationLedger item);
}

public sealed record SeasonalNominationLedgerBudgetRangeRule(double Min, double Max) : ISeasonalNominationLedgerRule
{
    public List<string> Check(SeasonalNominationLedger item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalNominationLedgerAllowedCodeRule(List<string> Allowed) : ISeasonalNominationLedgerRule
{
    public List<string> Check(SeasonalNominationLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalNominationLedgerCodeLengthRule(int MaxLength) : ISeasonalNominationLedgerRule
{
    public List<string> Check(SeasonalNominationLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalNominationLedgerExpiresAtRangeRule(int Min, int Max) : ISeasonalNominationLedgerRule
{
    public List<string> Check(SeasonalNominationLedger item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalNominationLedgerRequiredLabelRule(string Message) : ISeasonalNominationLedgerRule
{
    public List<string> Check(SeasonalNominationLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal nomination ledger.</summary>
public sealed record SeasonalNominationLedgerValidator(List<ISeasonalNominationLedgerRule> Rules)
{
    public List<string> Validate(SeasonalNominationLedger item)
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

    public void RequireValid(SeasonalNominationLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalNominationLedger> items)
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

/// <summary>Functions of seasonal nomination ledger rules.</summary>
public static class SeasonalNominationLedgerRules
{
    public static SeasonalNominationLedgerValidator DefaultSeasonalNominationLedgerValidator()
    {
        return new SeasonalNominationLedgerValidator(
            new List<ISeasonalNominationLedgerRule>
            {
                new SeasonalNominationLedgerBudgetRangeRule(7.0, 2499.8),
                new SeasonalNominationLedgerAllowedCodeRule(new List<string> { "seasonal", "strict" }),
                new SeasonalNominationLedgerCodeLengthRule(3600),
                new SeasonalNominationLedgerExpiresAtRangeRule(80, 3600),
                new SeasonalNominationLedgerRequiredLabelRule("seasonal nomination ledger needs this field"),
            });
    }
}
