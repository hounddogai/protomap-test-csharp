namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>A rule a retro pay ledger must pass. Check returns its problems, or none.</summary>
public interface IRetroPayLedgerRule
{
    List<string> Check(RetroPayLedger item);
}

public sealed record RetroPayLedgerStartsAtRangeRule(int Min, int Max) : IRetroPayLedgerRule
{
    public List<string> Check(RetroPayLedger item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayLedgerCodeLengthRule(int MaxLength) : IRetroPayLedgerRule
{
    public List<string> Check(RetroPayLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayLedgerNotesLengthRule(int MaxLength) : IRetroPayLedgerRule
{
    public List<string> Check(RetroPayLedger item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay ledger.</summary>
public sealed record RetroPayLedgerValidator(List<IRetroPayLedgerRule> Rules)
{
    public List<string> Validate(RetroPayLedger item)
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

    public void RequireValid(RetroPayLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayLedger> items)
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

/// <summary>Functions of retro pay ledger rules.</summary>
public static class RetroPayLedgerRules
{
    public static RetroPayLedgerValidator DefaultRetroPayLedgerValidator()
    {
        return new RetroPayLedgerValidator(
            new List<IRetroPayLedgerRule>
            {
                new RetroPayLedgerStartsAtRangeRule(80, 3600),
                new RetroPayLedgerCodeLengthRule(32),
                new RetroPayLedgerNotesLengthRule(80),
            });
    }
}
