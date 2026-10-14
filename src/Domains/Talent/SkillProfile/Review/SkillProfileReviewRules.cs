namespace Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>A rule a skill profile review must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileReviewRule
{
    List<string> Check(SkillProfileReview item);
}

public sealed record SkillProfileReviewRequiredReviewerRule(string Message) : ISkillProfileReviewRule
{
    public List<string> Check(SkillProfileReview item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileReviewReviewerLengthRule(int MaxLength) : ISkillProfileReviewRule
{
    public List<string> Check(SkillProfileReview item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileReviewAttemptsRangeRule(int Min, int Max) : ISkillProfileReviewRule
{
    public List<string> Check(SkillProfileReview item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile review.</summary>
public sealed record SkillProfileReviewValidator(List<ISkillProfileReviewRule> Rules)
{
    public List<string> Validate(SkillProfileReview item)
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

    public void RequireValid(SkillProfileReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileReview> items)
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

/// <summary>Functions of skill profile review rules.</summary>
public static class SkillProfileReviewRules
{
    public static SkillProfileReviewValidator DefaultSkillProfileReviewValidator()
    {
        return new SkillProfileReviewValidator(
            new List<ISkillProfileReviewRule>
            {
                new SkillProfileReviewRequiredReviewerRule("skill profile review needs this field"),
                new SkillProfileReviewReviewerLengthRule(1000),
                new SkillProfileReviewAttemptsRangeRule(16, 80),
            });
    }
}
