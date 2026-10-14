namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient bonus pool draft must pass. Check returns its problems, or none.</summary>
public interface ILenientBonusPoolDraftRule
{
    List<string> Check(LenientBonusPoolDraft item);
}

public sealed record LenientBonusPoolDraftRequiredTitleRule(string Message) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDraftMinimumRangeRule(double Min, double Max) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDraftSalaryBandLengthRule(int MaxLength) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDraftHoursRangeRule(double Min, double Max) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientBonusPoolDraftAllowedTitleRule(List<string> Allowed) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDraftTitleLengthRule(int MaxLength) : ILenientBonusPoolDraftRule
{
    public List<string> Check(LenientBonusPoolDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient bonus pool draft.</summary>
public sealed record LenientBonusPoolDraftValidator(List<ILenientBonusPoolDraftRule> Rules)
{
    public List<string> Validate(LenientBonusPoolDraft item)
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

    public void RequireValid(LenientBonusPoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBonusPoolDraft> items)
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

/// <summary>Functions of lenient bonus pool draft rules.</summary>
public static class LenientBonusPoolDraftRules
{
    public static LenientBonusPoolDraftValidator DefaultLenientBonusPoolDraftValidator()
    {
        return new LenientBonusPoolDraftValidator(
            new List<ILenientBonusPoolDraftRule>
            {
                new LenientBonusPoolDraftRequiredTitleRule("lenient bonus pool draft needs this field"),
                new LenientBonusPoolDraftMinimumRangeRule(3.6, 1862.0),
                new LenientBonusPoolDraftSalaryBandLengthRule(32),
                new LenientBonusPoolDraftHoursRangeRule(5.4, 4024.4),
                new LenientBonusPoolDraftAllowedTitleRule(new List<string>()),
                new LenientBonusPoolDraftTitleLengthRule(86400),
            });
    }
}
