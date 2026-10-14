namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>A rule a capped cost model draft must pass. Check returns its problems, or none.</summary>
public interface ICappedCostModelDraftRule
{
    List<string> Check(CappedCostModelDraft item);
}

public sealed record CappedCostModelDraftRequiredSkillRule(string Message) : ICappedCostModelDraftRule
{
    public List<string> Check(CappedCostModelDraft item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelDraftCodePrefixRule(string Prefix) : ICappedCostModelDraftRule
{
    public List<string> Check(CappedCostModelDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelDraftMarginRangeRule(double Min, double Max) : ICappedCostModelDraftRule
{
    public List<string> Check(CappedCostModelDraft item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelDraftSkillLengthRule(int MaxLength) : ICappedCostModelDraftRule
{
    public List<string> Check(CappedCostModelDraft item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelDraftRequiredReviewerRule(string Message) : ICappedCostModelDraftRule
{
    public List<string> Check(CappedCostModelDraft item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped cost model draft.</summary>
public sealed record CappedCostModelDraftValidator(List<ICappedCostModelDraftRule> Rules)
{
    public List<string> Validate(CappedCostModelDraft item)
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

    public void RequireValid(CappedCostModelDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCostModelDraft> items)
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

/// <summary>Functions of capped cost model draft rules.</summary>
public static class CappedCostModelDraftRules
{
    public static CappedCostModelDraftValidator DefaultCappedCostModelDraftValidator()
    {
        return new CappedCostModelDraftValidator(
            new List<ICappedCostModelDraftRule>
            {
                new CappedCostModelDraftRequiredSkillRule("capped cost model draft needs this field"),
                new CappedCostModelDraftCodePrefixRule("ACM"),
                new CappedCostModelDraftMarginRangeRule(3.2, 4398.6),
                new CappedCostModelDraftSkillLengthRule(16),
                new CappedCostModelDraftRequiredReviewerRule("capped cost model draft needs an entry"),
            });
    }
}
