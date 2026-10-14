namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient deduction draft must pass. Check returns its problems, or none.</summary>
public interface ILenientDeductionDraftRule
{
    List<string> Check(LenientDeductionDraft item);
}

public sealed record LenientDeductionDraftRequiredCategoryRule(string Message) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionDraftCodeLengthRule(int MaxLength) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionDraftCodePrefixRule(string Prefix) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionDraftAllowedReviewerRule(List<string> Allowed) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionDraftFloorValueRangeRule(double Min, double Max) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionDraftNameLengthRule(int MaxLength) : ILenientDeductionDraftRule
{
    public List<string> Check(LenientDeductionDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient deduction draft.</summary>
public sealed record LenientDeductionDraftValidator(List<ILenientDeductionDraftRule> Rules)
{
    public List<string> Validate(LenientDeductionDraft item)
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

    public void RequireValid(LenientDeductionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeductionDraft> items)
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

/// <summary>Functions of lenient deduction draft rules.</summary>
public static class LenientDeductionDraftRules
{
    public static LenientDeductionDraftValidator DefaultLenientDeductionDraftValidator()
    {
        return new LenientDeductionDraftValidator(
            new List<ILenientDeductionDraftRule>
            {
                new LenientDeductionDraftRequiredCategoryRule("lenient deduction draft needs a value"),
                new LenientDeductionDraftCodeLengthRule(255),
                new LenientDeductionDraftCodePrefixRule("REF"),
                new LenientDeductionDraftAllowedReviewerRule(new List<string> { "primary" }),
                new LenientDeductionDraftFloorValueRangeRule(3.0, 2338.5),
                new LenientDeductionDraftNameLengthRule(80),
            });
    }
}
