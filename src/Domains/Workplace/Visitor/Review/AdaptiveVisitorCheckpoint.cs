namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive visitor checkpoint is in its life cycle.</summary>
public enum AdaptiveVisitorCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive visitor checkpoint and its current status.</summary>
public sealed class AdaptiveVisitorCheckpoint(string code, string title, string skill, string country, string managerId, int graceDays, string checksum, string region, string jobCode, int limit, AdaptiveVisitorCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public string Checksum { get; set; } = checksum;
    public string Region { get; set; } = region;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public AdaptiveVisitorCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive visitor checkpoint in status requested.</summary>
    public static AdaptiveVisitorCheckpoint NewAdaptiveVisitorCheckpoint(string code, string title)
    {
        return new AdaptiveVisitorCheckpoint(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            "",
            "",
            0,
            AdaptiveVisitorCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveVisitorCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveVisitorCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveVisitorCheckpointStatus.Requested:
                switch (next)
                {
                    case AdaptiveVisitorCheckpointStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveVisitorCheckpointStatus.Granted:
                return next == AdaptiveVisitorCheckpointStatus.Expired;
            case AdaptiveVisitorCheckpointStatus.Revoked:
                return next == AdaptiveVisitorCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveVisitorCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive visitor checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Limit + (double)GraceDays, 2);
    }

    public AdaptiveVisitorCheckpoint Copy()
    {
        return new AdaptiveVisitorCheckpoint(
            Code,
            Title,
            Skill,
            Country,
            ManagerId,
            GraceDays,
            Checksum,
            Region,
            JobCode,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
