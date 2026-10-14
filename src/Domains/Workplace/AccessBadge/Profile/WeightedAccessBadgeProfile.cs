namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted access badge profile is in its life cycle.</summary>
public enum WeightedAccessBadgeProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted access badge profile. Its code is unique within a tenant.</summary>
public sealed class WeightedAccessBadgeProfile(string code, string label, string skill, string grade, int quantity, double maximum, string source, bool visible, string channel, WeightedAccessBadgeProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public string Source { get; set; } = source;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public WeightedAccessBadgeProfileStatus Status { get; set; } = status;

    /// <summary>Starts a weighted access badge profile in status open.</summary>
    public static WeightedAccessBadgeProfile NewWeightedAccessBadgeProfile(string code, string label)
    {
        return new WeightedAccessBadgeProfile(
            code,
            label,
            "",
            "",
            0,
            0.0,
            "",
            false,
            "",
            WeightedAccessBadgeProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccessBadgeProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccessBadgeProfileStatus next)
    {
        switch (Status)
        {
            case WeightedAccessBadgeProfileStatus.Open:
                switch (next)
                {
                    case WeightedAccessBadgeProfileStatus.Resolved:
                        return true;
                }
                return false;
            case WeightedAccessBadgeProfileStatus.InReview:
                switch (next)
                {
                    case WeightedAccessBadgeProfileStatus.Archived:
                        return true;
                }
                return false;
            case WeightedAccessBadgeProfileStatus.Resolved:
                switch (next)
                {
                    case WeightedAccessBadgeProfileStatus.Reopened:
                    case WeightedAccessBadgeProfileStatus.Archived:
                        return true;
                }
                return false;
            case WeightedAccessBadgeProfileStatus.Reopened:
                return next == WeightedAccessBadgeProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedAccessBadgeProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted access badge profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted access badge profile"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Maximum - (double)Quantity, 2);
    }

    public WeightedAccessBadgeProfile Copy()
    {
        return new WeightedAccessBadgeProfile(
            Code,
            Label,
            Skill,
            Grade,
            Quantity,
            Maximum,
            Source,
            Visible,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
