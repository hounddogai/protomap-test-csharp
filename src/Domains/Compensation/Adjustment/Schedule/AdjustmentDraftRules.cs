namespace Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>A rule a adjustment draft must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentDraftRule
{
    List<string> Check(AdjustmentDraft item);
}

public sealed record AdjustmentDraftCodeLengthRule(int MaxLength) : IAdjustmentDraftRule
{
    public List<string> Check(AdjustmentDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentDraftRequiredCodeRule(string Message) : IAdjustmentDraftRule
{
    public List<string> Check(AdjustmentDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentDraftRequiredDepartmentRule(string Message) : IAdjustmentDraftRule
{
    public List<string> Check(AdjustmentDraft item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentDraftCodePrefixRule(string Prefix) : IAdjustmentDraftRule
{
    public List<string> Check(AdjustmentDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentDraftCostRangeRule(double Min, double Max) : IAdjustmentDraftRule
{
    public List<string> Check(AdjustmentDraft item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment draft.</summary>
public sealed record AdjustmentDraftValidator(List<IAdjustmentDraftRule> Rules)
{
    public List<string> Validate(AdjustmentDraft item)
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

    public void RequireValid(AdjustmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentDraft> items)
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

/// <summary>Functions of adjustment draft rules.</summary>
public static class AdjustmentDraftRules
{
    public static AdjustmentDraftValidator DefaultAdjustmentDraftValidator()
    {
        return new AdjustmentDraftValidator(
            new List<IAdjustmentDraftRule>
            {
                new AdjustmentDraftCodeLengthRule(64),
                new AdjustmentDraftRequiredCodeRule("adjustment draft needs an entry"),
                new AdjustmentDraftRequiredDepartmentRule("adjustment draft needs a value"),
                new AdjustmentDraftCodePrefixRule("OPS"),
                new AdjustmentDraftCostRangeRule(3.7, 2291.2),
            });
    }
}
