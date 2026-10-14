namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict town hall ledger must pass. Check returns its problems, or none.</summary>
public interface IStrictTownHallLedgerRule
{
    List<string> Check(StrictTownHallLedger item);
}

public sealed record StrictTownHallLedgerAllowedNameRule(List<string> Allowed) : IStrictTownHallLedgerRule
{
    public List<string> Check(StrictTownHallLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record StrictTownHallLedgerCodeLengthRule(int MaxLength) : IStrictTownHallLedgerRule
{
    public List<string> Check(StrictTownHallLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTownHallLedgerSourceLengthRule(int MaxLength) : IStrictTownHallLedgerRule
{
    public List<string> Check(StrictTownHallLedger item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTownHallLedgerLeaveDaysRangeRule(int Min, int Max) : IStrictTownHallLedgerRule
{
    public List<string> Check(StrictTownHallLedger item)
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

public sealed record StrictTownHallLedgerRequiredNameRule(string Message) : IStrictTownHallLedgerRule
{
    public List<string> Check(StrictTownHallLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict town hall ledger.</summary>
public sealed record StrictTownHallLedgerValidator(List<IStrictTownHallLedgerRule> Rules)
{
    public List<string> Validate(StrictTownHallLedger item)
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

    public void RequireValid(StrictTownHallLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTownHallLedger> items)
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

/// <summary>Functions of strict town hall ledger rules.</summary>
public static class StrictTownHallLedgerRules
{
    public static StrictTownHallLedgerValidator DefaultStrictTownHallLedgerValidator()
    {
        return new StrictTownHallLedgerValidator(
            new List<IStrictTownHallLedgerRule>
            {
                new StrictTownHallLedgerAllowedNameRule(new List<string>()),
                new StrictTownHallLedgerCodeLengthRule(255),
                new StrictTownHallLedgerSourceLengthRule(80),
                new StrictTownHallLedgerLeaveDaysRangeRule(16, 64),
                new StrictTownHallLedgerRequiredNameRule("strict town hall ledger needs a value"),
            });
    }
}
