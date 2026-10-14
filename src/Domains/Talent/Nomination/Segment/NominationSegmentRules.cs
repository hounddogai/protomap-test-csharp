namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination segment must pass. Check returns its problems, or none.</summary>
public interface INominationSegmentRule
{
    List<string> Check(NominationSegment item);
}

public sealed record NominationSegmentCategoryLengthRule(int MaxLength) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSegmentCodeLengthRule(int MaxLength) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSegmentAllowedSalaryBandRule(List<string> Allowed) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record NominationSegmentRequiredCategoryRule(string Message) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationSegmentTitleLengthRule(int MaxLength) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationSegmentRequiredJobCodeRule(string Message) : INominationSegmentRule
{
    public List<string> Check(NominationSegment item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination segment.</summary>
public sealed record NominationSegmentValidator(List<INominationSegmentRule> Rules)
{
    public List<string> Validate(NominationSegment item)
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

    public void RequireValid(NominationSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationSegment> items)
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

/// <summary>Functions of nomination segment rules.</summary>
public static class NominationSegmentRules
{
    public static NominationSegmentValidator DefaultNominationSegmentValidator()
    {
        return new NominationSegmentValidator(
            new List<INominationSegmentRule>
            {
                new NominationSegmentCategoryLengthRule(120),
                new NominationSegmentCodeLengthRule(16),
                new NominationSegmentAllowedSalaryBandRule(new List<string>()),
                new NominationSegmentRequiredCategoryRule("nomination segment needs a value"),
                new NominationSegmentTitleLengthRule(16),
                new NominationSegmentRequiredJobCodeRule("nomination segment needs an entry"),
            });
    }
}
