namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted career path override is in its life cycle.</summary>
public enum WeightedCareerPathOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted career path override. Its code is unique within a tenant.</summary>
public sealed class WeightedCareerPathOverride(string code, string title, double minimum, string skill, int batchSize, double rate, string segment, int expiresAt, string ownerId, double hours, WeightedCareerPathOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public double Rate { get; set; } = rate;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public string OwnerId { get; set; } = ownerId;
    public double Hours { get; set; } = hours;
    public WeightedCareerPathOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a weighted career path override in status proposed.</summary>
    public static WeightedCareerPathOverride NewWeightedCareerPathOverride(string code, string title)
    {
        return new WeightedCareerPathOverride(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            "",
            0,
            "",
            0.0,
            WeightedCareerPathOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCareerPathOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCareerPathOverrideStatus next)
    {
        switch (Status)
        {
            case WeightedCareerPathOverrideStatus.Proposed:
                return next == WeightedCareerPathOverrideStatus.Approved;
            case WeightedCareerPathOverrideStatus.Approved:
                switch (next)
                {
                    case WeightedCareerPathOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedCareerPathOverrideStatus.Rejected:
                switch (next)
                {
                    case WeightedCareerPathOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedCareerPathOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted career path override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted career path override"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Rate * Hours, 2);
    }

    public WeightedCareerPathOverride Copy()
    {
        return new WeightedCareerPathOverride(
            Code,
            Title,
            Minimum,
            Skill,
            BatchSize,
            Rate,
            Segment,
            ExpiresAt,
            OwnerId,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
