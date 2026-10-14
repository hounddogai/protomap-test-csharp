namespace Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>A rule a assignment review must pass. Check returns its problems, or none.</summary>
public interface IAssignmentReviewRule
{
    List<string> Check(AssignmentReview item);
}

public sealed record AssignmentReviewTitleLengthRule(int MaxLength) : IAssignmentReviewRule
{
    public List<string> Check(AssignmentReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReviewGraceDaysRangeRule(int Min, int Max) : IAssignmentReviewRule
{
    public List<string> Check(AssignmentReview item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReviewRequiredContactEmailRule(string Message) : IAssignmentReviewRule
{
    public List<string> Check(AssignmentReview item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReviewTeamLengthRule(int MaxLength) : IAssignmentReviewRule
{
    public List<string> Check(AssignmentReview item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentReviewCodeLengthRule(int MaxLength) : IAssignmentReviewRule
{
    public List<string> Check(AssignmentReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment review.</summary>
public sealed record AssignmentReviewValidator(List<IAssignmentReviewRule> Rules)
{
    public List<string> Validate(AssignmentReview item)
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

    public void RequireValid(AssignmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentReview> items)
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

/// <summary>Functions of assignment review rules.</summary>
public static class AssignmentReviewRules
{
    public static AssignmentReviewValidator DefaultAssignmentReviewValidator()
    {
        return new AssignmentReviewValidator(
            new List<IAssignmentReviewRule>
            {
                new AssignmentReviewTitleLengthRule(3600),
                new AssignmentReviewGraceDaysRangeRule(120, 1000),
                new AssignmentReviewRequiredContactEmailRule("assignment review needs this field"),
                new AssignmentReviewTeamLengthRule(120),
                new AssignmentReviewCodeLengthRule(120),
            });
    }
}
