namespace Acme.Workforce.Engagement.Recognition.Ledger;

/// <summary>A rule a recognition ledger must pass. Check returns its problems, or none.</summary>
public interface IRecognitionLedgerRule
{
    List<string> Check(RecognitionLedger item);
}

public sealed record RecognitionLedgerRequiredCodeRule(string Message) : IRecognitionLedgerRule
{
    public List<string> Check(RecognitionLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionLedgerCodePrefixRule(string Prefix) : IRecognitionLedgerRule
{
    public List<string> Check(RecognitionLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionLedgerPriorityRangeRule(int Min, int Max) : IRecognitionLedgerRule
{
    public List<string> Check(RecognitionLedger item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionLedgerLeaveDaysRangeRule(int Min, int Max) : IRecognitionLedgerRule
{
    public List<string> Check(RecognitionLedger item)
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

/// <summary>Runs every rule of a recognition ledger.</summary>
public sealed record RecognitionLedgerValidator(List<IRecognitionLedgerRule> Rules)
{
    public List<string> Validate(RecognitionLedger item)
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

    public void RequireValid(RecognitionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionLedger> items)
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

/// <summary>Functions of recognition ledger rules.</summary>
public static class RecognitionLedgerRules
{
    public static RecognitionLedgerValidator DefaultRecognitionLedgerValidator()
    {
        return new RecognitionLedgerValidator(
            new List<IRecognitionLedgerRule>
            {
                new RecognitionLedgerRequiredCodeRule("recognition ledger needs an entry"),
                new RecognitionLedgerCodePrefixRule("REF"),
                new RecognitionLedgerPriorityRangeRule(80, 86400),
                new RecognitionLedgerLeaveDaysRangeRule(16, 1000),
            });
    }
}
