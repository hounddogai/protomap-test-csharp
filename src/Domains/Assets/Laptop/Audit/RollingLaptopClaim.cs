namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling laptop claim is in its life cycle.</summary>
public enum RollingLaptopClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One rolling laptop claim, with what the planners need to act on it.</summary>
public sealed class RollingLaptopClaim(string code, string name, string jobCode, int windowDays, string phone, int startsAt, string notes, int retries, string grade, bool active, RollingLaptopClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public int StartsAt { get; set; } = startsAt;
    public string Notes { get; set; } = notes;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public RollingLaptopClaimStatus Status { get; set; } = status;

    /// <summary>Starts a rolling laptop claim in status open.</summary>
    public static RollingLaptopClaim NewRollingLaptopClaim(string code, string name)
    {
        return new RollingLaptopClaim(code, name, "", 0, "", 0, "", 0, "", false, RollingLaptopClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLaptopClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLaptopClaimStatus next)
    {
        switch (Status)
        {
            case RollingLaptopClaimStatus.Open:
                switch (next)
                {
                    case RollingLaptopClaimStatus.InReview:
                        return true;
                }
                return false;
            case RollingLaptopClaimStatus.InReview:
                return next == RollingLaptopClaimStatus.Resolved || next == RollingLaptopClaimStatus.Archived;
            case RollingLaptopClaimStatus.Resolved:
                return next == RollingLaptopClaimStatus.Reopened || next == RollingLaptopClaimStatus.Archived;
            case RollingLaptopClaimStatus.Reopened:
                return next == RollingLaptopClaimStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingLaptopClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling laptop claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling laptop claim"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays - (double)StartsAt, 2);
    }

    public RollingLaptopClaim Copy()
    {
        return new RollingLaptopClaim(
            Code,
            Name,
            JobCode,
            WindowDays,
            Phone,
            StartsAt,
            Notes,
            Retries,
            Grade,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
