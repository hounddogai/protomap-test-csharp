namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile review is in its life cycle.</summary>
public enum SkillProfileReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A skill profile review. Its code is unique within a tenant.</summary>
public sealed class SkillProfileReview(string code, string name, int windowDays, string reviewer, double minimum, int attempts, string title, int limit, double amount, string notes, string department, SkillProfileReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public double Minimum { get; set; } = minimum;
    public int Attempts { get; set; } = attempts;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public double Amount { get; set; } = amount;
    public string Notes { get; set; } = notes;
    public string Department { get; set; } = department;
    public SkillProfileReviewStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile review in status open.</summary>
    public static SkillProfileReview NewSkillProfileReview(string code, string name)
    {
        return new SkillProfileReview(code, name, 0, "", 0.0, 0, "", 0, 0.0, "", "", SkillProfileReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileReviewStatus next)
    {
        switch (Status)
        {
            case SkillProfileReviewStatus.Open:
                switch (next)
                {
                    case SkillProfileReviewStatus.Resolved:
                        return true;
                }
                return false;
            case SkillProfileReviewStatus.InReview:
                return next == SkillProfileReviewStatus.Resolved;
            case SkillProfileReviewStatus.Resolved:
                return next == SkillProfileReviewStatus.Archived;
            case SkillProfileReviewStatus.Reopened:
                return next == SkillProfileReviewStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SkillProfileReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit + Minimum, 2);
    }

    public SkillProfileReview Copy()
    {
        return new SkillProfileReview(
            Code,
            Name,
            WindowDays,
            Reviewer,
            Minimum,
            Attempts,
            Title,
            Limit,
            Amount,
            Notes,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
