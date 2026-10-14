namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback leave request template is in its life cycle.</summary>
public enum FallbackLeaveRequestTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback leave request template as the leave request team keeps it.</summary>
public sealed class FallbackLeaveRequestTemplate(string code, string name, int durationMinutes, double floorValue, string grade, string notes, string timezone, string checksum, string region, FallbackLeaveRequestTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public string Timezone { get; set; } = timezone;
    public string Checksum { get; set; } = checksum;
    public string Region { get; set; } = region;
    public FallbackLeaveRequestTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a fallback leave request template in status open.</summary>
    public static FallbackLeaveRequestTemplate NewFallbackLeaveRequestTemplate(string code, string name)
    {
        return new FallbackLeaveRequestTemplate(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            "",
            "",
            FallbackLeaveRequestTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLeaveRequestTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLeaveRequestTemplateStatus next)
    {
        switch (Status)
        {
            case FallbackLeaveRequestTemplateStatus.Open:
                switch (next)
                {
                    case FallbackLeaveRequestTemplateStatus.Resolved:
                        return true;
                }
                return false;
            case FallbackLeaveRequestTemplateStatus.InReview:
                return next == FallbackLeaveRequestTemplateStatus.Resolved || next == FallbackLeaveRequestTemplateStatus.Archived;
            case FallbackLeaveRequestTemplateStatus.Resolved:
                switch (next)
                {
                    case FallbackLeaveRequestTemplateStatus.Reopened:
                    case FallbackLeaveRequestTemplateStatus.Archived:
                        return true;
                }
                return false;
            case FallbackLeaveRequestTemplateStatus.Reopened:
                switch (next)
                {
                    case FallbackLeaveRequestTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLeaveRequestTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback leave request template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes + FloorValue, 2);
    }

    public FallbackLeaveRequestTemplate Copy()
    {
        return new FallbackLeaveRequestTemplate(
            Code,
            Name,
            DurationMinutes,
            FloorValue,
            Grade,
            Notes,
            Timezone,
            Checksum,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
