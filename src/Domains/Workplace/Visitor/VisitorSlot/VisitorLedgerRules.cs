namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor ledger must pass. Check returns its problems, or none.</summary>
public interface IVisitorLedgerRule
{
    List<string> Check(VisitorLedger item);
}

public sealed record VisitorLedgerLeaveDaysRangeRule(int Min, int Max) : IVisitorLedgerRule
{
    public List<string> Check(VisitorLedger item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorLedgerAllowedTitleRule(List<string> Allowed) : IVisitorLedgerRule
{
    public List<string> Check(VisitorLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record VisitorLedgerRequiredCodeRule(string Message) : IVisitorLedgerRule
{
    public List<string> Check(VisitorLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor ledger.</summary>
public sealed record VisitorLedgerValidator(List<IVisitorLedgerRule> Rules)
{
    public List<string> Validate(VisitorLedger item)
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

    public void RequireValid(VisitorLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorLedger> items)
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

/// <summary>Functions of visitor ledger rules.</summary>
public static class VisitorLedgerRules
{
    public static VisitorLedgerValidator DefaultVisitorLedgerValidator()
    {
        return new VisitorLedgerValidator(
            new List<IVisitorLedgerRule>
            {
                new VisitorLedgerLeaveDaysRangeRule(32, 3600),
                new VisitorLedgerAllowedTitleRule(new List<string> { "pending", "weighted" }),
                new VisitorLedgerRequiredCodeRule("visitor ledger needs a value"),
            });
    }
}
