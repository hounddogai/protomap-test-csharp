namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay band mapping is in its life cycle.</summary>
public enum FallbackPayBandMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback pay band mapping. Its code is unique within a tenant.</summary>
public sealed class FallbackPayBandMapping(string code, string title, string managerId, string salaryBand, int durationMinutes, FallbackPayBandMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public FallbackPayBandMappingStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay band mapping in status new.</summary>
    public static FallbackPayBandMapping NewFallbackPayBandMapping(string code, string title)
    {
        return new FallbackPayBandMapping(code, title, "", "", 0, FallbackPayBandMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayBandMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayBandMappingStatus next)
    {
        switch (Status)
        {
            case FallbackPayBandMappingStatus.New:
                switch (next)
                {
                    case FallbackPayBandMappingStatus.Done:
                    case FallbackPayBandMappingStatus.Failed:
                        return true;
                }
                return false;
            case FallbackPayBandMappingStatus.Queued:
                return next == FallbackPayBandMappingStatus.Done || next == FallbackPayBandMappingStatus.Failed;
            case FallbackPayBandMappingStatus.Running:
                switch (next)
                {
                    case FallbackPayBandMappingStatus.Done:
                        return true;
                }
                return false;
            case FallbackPayBandMappingStatus.Done:
                switch (next)
                {
                    case FallbackPayBandMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPayBandMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay band mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public FallbackPayBandMapping Copy()
    {
        return new FallbackPayBandMapping(Code, Title, ManagerId, SalaryBand, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
