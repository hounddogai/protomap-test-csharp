namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception claim is in its life cycle.</summary>
public enum ExceptionClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception claim. Its code is unique within a tenant.</summary>
public sealed class ExceptionClaim(string code, string name, string channel, int capacity, int version, int createdAt, string reviewer, string unit, bool pinned, string contactEmail, double fte, ExceptionClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public string Reviewer { get; set; } = reviewer;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public double Fte { get; set; } = fte;
    public ExceptionClaimStatus Status { get; set; } = status;

    /// <summary>Starts a exception claim in status draft.</summary>
    public static ExceptionClaim NewExceptionClaim(string code, string name)
    {
        return new ExceptionClaim(code, name, "", 0, 0, 0, "", "", false, "", 0.0, ExceptionClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionClaimStatus next)
    {
        switch (Status)
        {
            case ExceptionClaimStatus.Draft:
                return next == ExceptionClaimStatus.Closed;
            case ExceptionClaimStatus.Pending:
                switch (next)
                {
                    case ExceptionClaimStatus.Suspended:
                    case ExceptionClaimStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionClaimStatus.Active:
                return next == ExceptionClaimStatus.Suspended || next == ExceptionClaimStatus.Closed;
            case ExceptionClaimStatus.Suspended:
                switch (next)
                {
                    case ExceptionClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version * (double)Capacity, 2);
    }

    public ExceptionClaim Copy()
    {
        return new ExceptionClaim(
            Code,
            Name,
            Channel,
            Capacity,
            Version,
            CreatedAt,
            Reviewer,
            Unit,
            Pinned,
            ContactEmail,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
