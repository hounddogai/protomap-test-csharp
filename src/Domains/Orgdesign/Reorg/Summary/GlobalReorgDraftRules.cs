namespace Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>A rule a global reorg draft must pass. Check returns its problems, or none.</summary>
public interface IGlobalReorgDraftRule
{
    List<string> Check(GlobalReorgDraft item);
}

public sealed record GlobalReorgDraftCodePrefixRule(string Prefix) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalReorgDraftRequiredDepartmentRule(string Message) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalReorgDraftRequiredChannelRule(string Message) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalReorgDraftBudgetRangeRule(double Min, double Max) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalReorgDraftBatchSizeRangeRule(int Min, int Max) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalReorgDraftChannelLengthRule(int MaxLength) : IGlobalReorgDraftRule
{
    public List<string> Check(GlobalReorgDraft item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global reorg draft.</summary>
public sealed record GlobalReorgDraftValidator(List<IGlobalReorgDraftRule> Rules)
{
    public List<string> Validate(GlobalReorgDraft item)
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

    public void RequireValid(GlobalReorgDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalReorgDraft> items)
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

/// <summary>Functions of global reorg draft rules.</summary>
public static class GlobalReorgDraftRules
{
    public static GlobalReorgDraftValidator DefaultGlobalReorgDraftValidator()
    {
        return new GlobalReorgDraftValidator(
            new List<IGlobalReorgDraftRule>
            {
                new GlobalReorgDraftCodePrefixRule("Q"),
                new GlobalReorgDraftRequiredDepartmentRule("global reorg draft needs this field"),
                new GlobalReorgDraftRequiredChannelRule("global reorg draft needs an entry"),
                new GlobalReorgDraftBudgetRangeRule(3.2, 595.3),
                new GlobalReorgDraftBatchSizeRangeRule(64, 1000),
                new GlobalReorgDraftChannelLengthRule(120),
            });
    }
}
