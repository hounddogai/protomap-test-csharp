namespace Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>A rule a adaptive deduction entry must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveDeductionEntryRule
{
    List<string> Check(AdaptiveDeductionEntry item);
}

public sealed record AdaptiveDeductionEntryRequiredCodeRule(string Message) : IAdaptiveDeductionEntryRule
{
    public List<string> Check(AdaptiveDeductionEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionEntryRequiredTitleRule(string Message) : IAdaptiveDeductionEntryRule
{
    public List<string> Check(AdaptiveDeductionEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionEntryCodeLengthRule(int MaxLength) : IAdaptiveDeductionEntryRule
{
    public List<string> Check(AdaptiveDeductionEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionEntryBudgetRangeRule(double Min, double Max) : IAdaptiveDeductionEntryRule
{
    public List<string> Check(AdaptiveDeductionEntry item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionEntrySequenceRangeRule(int Min, int Max) : IAdaptiveDeductionEntryRule
{
    public List<string> Check(AdaptiveDeductionEntry item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive deduction entry.</summary>
public sealed record AdaptiveDeductionEntryValidator(List<IAdaptiveDeductionEntryRule> Rules)
{
    public List<string> Validate(AdaptiveDeductionEntry item)
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

    public void RequireValid(AdaptiveDeductionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveDeductionEntry> items)
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

/// <summary>Functions of adaptive deduction entry rules.</summary>
public static class AdaptiveDeductionEntryRules
{
    public static AdaptiveDeductionEntryValidator DefaultAdaptiveDeductionEntryValidator()
    {
        return new AdaptiveDeductionEntryValidator(
            new List<IAdaptiveDeductionEntryRule>
            {
                new AdaptiveDeductionEntryRequiredCodeRule("adaptive deduction entry needs an entry"),
                new AdaptiveDeductionEntryRequiredTitleRule("adaptive deduction entry needs this field"),
                new AdaptiveDeductionEntryCodeLengthRule(64),
                new AdaptiveDeductionEntryBudgetRangeRule(2.7, 4837.0),
                new AdaptiveDeductionEntrySequenceRangeRule(16, 1000),
            });
    }
}
