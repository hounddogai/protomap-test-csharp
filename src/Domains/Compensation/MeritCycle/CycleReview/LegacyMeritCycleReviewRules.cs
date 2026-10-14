namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy merit cycle review must pass. Check returns its problems, or none.</summary>
public interface ILegacyMeritCycleReviewRule
{
    List<string> Check(LegacyMeritCycleReview item);
}

public sealed record LegacyMeritCycleReviewAllowedOwnerIdRule(List<string> Allowed) : ILegacyMeritCycleReviewRule
{
    public List<string> Check(LegacyMeritCycleReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleReviewRequiredNameRule(string Message) : ILegacyMeritCycleReviewRule
{
    public List<string> Check(LegacyMeritCycleReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleReviewPinnedGuardRule(int Limit) : ILegacyMeritCycleReviewRule
{
    public List<string> Check(LegacyMeritCycleReview item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleReviewLeaveDaysRangeRule(int Min, int Max) : ILegacyMeritCycleReviewRule
{
    public List<string> Check(LegacyMeritCycleReview item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyMeritCycleReviewRequiredOwnerIdRule(string Message) : ILegacyMeritCycleReviewRule
{
    public List<string> Check(LegacyMeritCycleReview item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy merit cycle review.</summary>
public sealed record LegacyMeritCycleReviewValidator(List<ILegacyMeritCycleReviewRule> Rules)
{
    public List<string> Validate(LegacyMeritCycleReview item)
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

    public void RequireValid(LegacyMeritCycleReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyMeritCycleReview> items)
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

/// <summary>Functions of legacy merit cycle review rules.</summary>
public static class LegacyMeritCycleReviewRules
{
    public static LegacyMeritCycleReviewValidator DefaultLegacyMeritCycleReviewValidator()
    {
        return new LegacyMeritCycleReviewValidator(
            new List<ILegacyMeritCycleReviewRule>
            {
                new LegacyMeritCycleReviewAllowedOwnerIdRule(new List<string> { "global", "seasonal", "default" }),
                new LegacyMeritCycleReviewRequiredNameRule("legacy merit cycle review needs a value"),
                new LegacyMeritCycleReviewPinnedGuardRule(86400),
                new LegacyMeritCycleReviewLeaveDaysRangeRule(16, 3600),
                new LegacyMeritCycleReviewRequiredOwnerIdRule("legacy merit cycle review needs an entry"),
            });
    }
}
