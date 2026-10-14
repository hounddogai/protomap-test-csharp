namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition record must pass. Check returns its problems, or none.</summary>
public interface IRecognitionRecordRule
{
    List<string> Check(RecognitionRecord item);
}

public sealed record RecognitionRecordRequiredCodeRule(string Message) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRecordLabelLengthRule(int MaxLength) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRecordCostCenterLengthRule(int MaxLength) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRecordAllowedLabelRule(List<string> Allowed) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRecordCodePrefixRule(string Prefix) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRecordRequiredCostCenterRule(string Message) : IRecognitionRecordRule
{
    public List<string> Check(RecognitionRecord item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition record.</summary>
public sealed record RecognitionRecordValidator(List<IRecognitionRecordRule> Rules)
{
    public List<string> Validate(RecognitionRecord item)
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

    public void RequireValid(RecognitionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionRecord> items)
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

/// <summary>Functions of recognition record rules.</summary>
public static class RecognitionRecordRules
{
    public static RecognitionRecordValidator DefaultRecognitionRecordValidator()
    {
        return new RecognitionRecordValidator(
            new List<IRecognitionRecordRule>
            {
                new RecognitionRecordRequiredCodeRule("recognition record needs an entry"),
                new RecognitionRecordLabelLengthRule(16),
                new RecognitionRecordCostCenterLengthRule(32),
                new RecognitionRecordAllowedLabelRule(new List<string> { "global", "pending" }),
                new RecognitionRecordCodePrefixRule("X"),
                new RecognitionRecordRequiredCostCenterRule("recognition record needs an entry"),
            });
    }
}
