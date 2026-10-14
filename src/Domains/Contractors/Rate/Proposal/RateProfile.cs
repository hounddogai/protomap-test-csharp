namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate profile is in its life cycle.</summary>
public enum RateProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rate profile. Its code is unique within a tenant.</summary>
public sealed class RateProfile(string code, string title, string reviewer, string channel, bool approved, string unit, string description, string source, string employeeId, double score, RateProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public bool Approved { get; set; } = approved;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public RateProfileStatus Status { get; set; } = status;

    /// <summary>Starts a rate profile in status new.</summary>
    public static RateProfile NewRateProfile(string code, string title)
    {
        return new RateProfile(code, title, "", "", false, "", "", "", "", 0.0, RateProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateProfileStatus next)
    {
        switch (Status)
        {
            case RateProfileStatus.New:
                return next == RateProfileStatus.Queued || next == RateProfileStatus.Running;
            case RateProfileStatus.Queued:
                switch (next)
                {
                    case RateProfileStatus.Done:
                        return true;
                }
                return false;
            case RateProfileStatus.Running:
                switch (next)
                {
                    case RateProfileStatus.Failed:
                        return true;
                }
                return false;
            case RateProfileStatus.Done:
                switch (next)
                {
                    case RateProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RateProfile Copy()
    {
        return new RateProfile(
            Code,
            Title,
            Reviewer,
            Channel,
            Approved,
            Unit,
            Description,
            Source,
            EmployeeId,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
