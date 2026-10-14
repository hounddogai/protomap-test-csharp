namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan request is in its life cycle.</summary>
public enum LenientSuccessionPlanRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient succession plan request as the succession plan team keeps it.</summary>
public sealed class LenientSuccessionPlanRequest(string code, string title, string ownerId, double maximum, string checksum, string timezone, string summary, string grade, double minimum, double floorValue, string description, LenientSuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public string Summary { get; set; } = summary;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public string Description { get; set; } = description;
    public LenientSuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan request in status proposed.</summary>
    public static LenientSuccessionPlanRequest NewLenientSuccessionPlanRequest(string code, string title)
    {
        return new LenientSuccessionPlanRequest(
            code,
            title,
            "",
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            LenientSuccessionPlanRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanRequestStatus.Proposed:
                return next == LenientSuccessionPlanRequestStatus.Approved;
            case LenientSuccessionPlanRequestStatus.Approved:
                return next == LenientSuccessionPlanRequestStatus.Withdrawn;
            case LenientSuccessionPlanRequestStatus.Rejected:
                return next == LenientSuccessionPlanRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient succession plan request"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(FloorValue + Maximum, 2);
    }

    public LenientSuccessionPlanRequest Copy()
    {
        return new LenientSuccessionPlanRequest(
            Code,
            Title,
            OwnerId,
            Maximum,
            Checksum,
            Timezone,
            Summary,
            Grade,
            Minimum,
            FloorValue,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
