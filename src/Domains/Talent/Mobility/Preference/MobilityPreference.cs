namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility preference is in its life cycle.</summary>
public enum MobilityPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A mobility preference. Its code is unique within a tenant.</summary>
public sealed class MobilityPreference(string code, string title, List<string> tags, string timezone, string badge, int tenureMonths, bool approved, string segment, double score, string grade, MobilityPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public string Badge { get; set; } = badge;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public MobilityPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a mobility preference in status open.</summary>
    public static MobilityPreference NewMobilityPreference(string code, string title)
    {
        return new MobilityPreference(
            code,
            title,
            new List<string>(),
            "",
            "",
            0,
            false,
            "",
            0.0,
            "",
            MobilityPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityPreferenceStatus next)
    {
        switch (Status)
        {
            case MobilityPreferenceStatus.Open:
                switch (next)
                {
                    case MobilityPreferenceStatus.InReview:
                        return true;
                }
                return false;
            case MobilityPreferenceStatus.InReview:
                switch (next)
                {
                    case MobilityPreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case MobilityPreferenceStatus.Resolved:
                return next == MobilityPreferenceStatus.Reopened;
            case MobilityPreferenceStatus.Reopened:
                switch (next)
                {
                    case MobilityPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)TenureMonths * Score, 2);
    }

    public MobilityPreference Copy()
    {
        return new MobilityPreference(
            Code,
            Title,
            Tags,
            Timezone,
            Badge,
            TenureMonths,
            Approved,
            Segment,
            Score,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
