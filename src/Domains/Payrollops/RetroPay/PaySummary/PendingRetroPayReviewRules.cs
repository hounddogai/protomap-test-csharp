namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>A rule a pending retro pay review must pass. Check returns its problems, or none.</summary>
public interface IPendingRetroPayReviewRule
{
    List<string> Check(PendingRetroPayReview item);
}

public sealed record PendingRetroPayReviewPinnedGuardRule(double Limit) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        if (item.Pinned && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayReviewRequiredCodeRule(string Message) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayReviewFloorValueRangeRule(double Min, double Max) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayReviewRequiredNameRule(string Message) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayReviewStartsAtRangeRule(int Min, int Max) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingRetroPayReviewNameLengthRule(int MaxLength) : IPendingRetroPayReviewRule
{
    public List<string> Check(PendingRetroPayReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending retro pay review.</summary>
public sealed record PendingRetroPayReviewValidator(List<IPendingRetroPayReviewRule> Rules)
{
    public List<string> Validate(PendingRetroPayReview item)
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

    public void RequireValid(PendingRetroPayReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRetroPayReview> items)
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

/// <summary>Functions of pending retro pay review rules.</summary>
public static class PendingRetroPayReviewRules
{
    public static PendingRetroPayReviewValidator DefaultPendingRetroPayReviewValidator()
    {
        return new PendingRetroPayReviewValidator(
            new List<IPendingRetroPayReviewRule>
            {
                new PendingRetroPayReviewPinnedGuardRule(1.1),
                new PendingRetroPayReviewRequiredCodeRule("pending retro pay review needs a value"),
                new PendingRetroPayReviewFloorValueRangeRule(0.4, 2019.7),
                new PendingRetroPayReviewRequiredNameRule("pending retro pay review needs a value"),
                new PendingRetroPayReviewStartsAtRangeRule(80, 86400),
                new PendingRetroPayReviewNameLengthRule(255),
            });
    }
}
