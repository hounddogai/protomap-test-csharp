namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>A rule a succession plan ledger must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanLedgerRule
{
    List<string> Check(SuccessionPlanLedger item);
}

public sealed record SuccessionPlanLedgerFloorValueRangeRule(double Min, double Max) : ISuccessionPlanLedgerRule
{
    public List<string> Check(SuccessionPlanLedger item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanLedgerLocaleLengthRule(int MaxLength) : ISuccessionPlanLedgerRule
{
    public List<string> Check(SuccessionPlanLedger item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanLedgerCodeLengthRule(int MaxLength) : ISuccessionPlanLedgerRule
{
    public List<string> Check(SuccessionPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanLedgerRequiredCodeRule(string Message) : ISuccessionPlanLedgerRule
{
    public List<string> Check(SuccessionPlanLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanLedgerRequiredTitleRule(string Message) : ISuccessionPlanLedgerRule
{
    public List<string> Check(SuccessionPlanLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan ledger.</summary>
public sealed record SuccessionPlanLedgerValidator(List<ISuccessionPlanLedgerRule> Rules)
{
    public List<string> Validate(SuccessionPlanLedger item)
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

    public void RequireValid(SuccessionPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanLedger> items)
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

/// <summary>Functions of succession plan ledger rules.</summary>
public static class SuccessionPlanLedgerRules
{
    public static SuccessionPlanLedgerValidator DefaultSuccessionPlanLedgerValidator()
    {
        return new SuccessionPlanLedgerValidator(
            new List<ISuccessionPlanLedgerRule>
            {
                new SuccessionPlanLedgerFloorValueRangeRule(5.6, 1924.8),
                new SuccessionPlanLedgerLocaleLengthRule(1000),
                new SuccessionPlanLedgerCodeLengthRule(120),
                new SuccessionPlanLedgerRequiredCodeRule("succession plan ledger needs an entry"),
                new SuccessionPlanLedgerRequiredTitleRule("succession plan ledger needs an entry"),
            });
    }
}
