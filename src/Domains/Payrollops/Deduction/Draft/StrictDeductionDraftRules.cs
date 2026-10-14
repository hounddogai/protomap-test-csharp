namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict deduction draft must pass. Check returns its problems, or none.</summary>
public interface IStrictDeductionDraftRule
{
    List<string> Check(StrictDeductionDraft item);
}

public sealed record StrictDeductionDraftCodeLengthRule(int MaxLength) : IStrictDeductionDraftRule
{
    public List<string> Check(StrictDeductionDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionDraftAllowedDescriptionRule(List<string> Allowed) : IStrictDeductionDraftRule
{
    public List<string> Check(StrictDeductionDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionDraftOwnerIdLengthRule(int MaxLength) : IStrictDeductionDraftRule
{
    public List<string> Check(StrictDeductionDraft item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionDraftRequiredCodeRule(string Message) : IStrictDeductionDraftRule
{
    public List<string> Check(StrictDeductionDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionDraftQuantityRangeRule(int Min, int Max) : IStrictDeductionDraftRule
{
    public List<string> Check(StrictDeductionDraft item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict deduction draft.</summary>
public sealed record StrictDeductionDraftValidator(List<IStrictDeductionDraftRule> Rules)
{
    public List<string> Validate(StrictDeductionDraft item)
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

    public void RequireValid(StrictDeductionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeductionDraft> items)
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

/// <summary>Functions of strict deduction draft rules.</summary>
public static class StrictDeductionDraftRules
{
    public static StrictDeductionDraftValidator DefaultStrictDeductionDraftValidator()
    {
        return new StrictDeductionDraftValidator(
            new List<IStrictDeductionDraftRule>
            {
                new StrictDeductionDraftCodeLengthRule(80),
                new StrictDeductionDraftAllowedDescriptionRule(new List<string>()),
                new StrictDeductionDraftOwnerIdLengthRule(120),
                new StrictDeductionDraftRequiredCodeRule("strict deduction draft needs a value"),
                new StrictDeductionDraftQuantityRangeRule(16, 64),
            });
    }
}
