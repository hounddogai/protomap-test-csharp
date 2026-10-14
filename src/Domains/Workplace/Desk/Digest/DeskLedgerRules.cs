namespace Acme.Workforce.Workplace.Desk.Digest;

/// <summary>A rule a desk ledger must pass. Check returns its problems, or none.</summary>
public interface IDeskLedgerRule
{
    List<string> Check(DeskLedger item);
}

public sealed record DeskLedgerRequiredGradeRule(string Message) : IDeskLedgerRule
{
    public List<string> Check(DeskLedger item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskLedgerActiveGuardRule(double Limit) : IDeskLedgerRule
{
    public List<string> Check(DeskLedger item)
    {
        if (item.Active && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record DeskLedgerDiscountRangeRule(double Min, double Max) : IDeskLedgerRule
{
    public List<string> Check(DeskLedger item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskLedgerTitleLengthRule(int MaxLength) : IDeskLedgerRule
{
    public List<string> Check(DeskLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk ledger.</summary>
public sealed record DeskLedgerValidator(List<IDeskLedgerRule> Rules)
{
    public List<string> Validate(DeskLedger item)
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

    public void RequireValid(DeskLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskLedger> items)
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

/// <summary>Functions of desk ledger rules.</summary>
public static class DeskLedgerRules
{
    public static DeskLedgerValidator DefaultDeskLedgerValidator()
    {
        return new DeskLedgerValidator(
            new List<IDeskLedgerRule>
            {
                new DeskLedgerRequiredGradeRule("desk ledger needs a value"),
                new DeskLedgerActiveGuardRule(7.9),
                new DeskLedgerDiscountRangeRule(6.4, 4882.0),
                new DeskLedgerTitleLengthRule(255),
            });
    }
}
