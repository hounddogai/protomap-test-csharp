namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat exception is in its life cycle.</summary>
public enum LicenseSeatExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A license seat exception. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatException(string code, string name, int expiresAt, string segment, double score, LicenseSeatExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public LicenseSeatExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a license seat exception in status draft.</summary>
    public static LicenseSeatException NewLicenseSeatException(string code, string name)
    {
        return new LicenseSeatException(code, name, 0, "", 0.0, LicenseSeatExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatExceptionStatus next)
    {
        switch (Status)
        {
            case LicenseSeatExceptionStatus.Draft:
                switch (next)
                {
                    case LicenseSeatExceptionStatus.Active:
                    case LicenseSeatExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case LicenseSeatExceptionStatus.Pending:
                return next == LicenseSeatExceptionStatus.Suspended;
            case LicenseSeatExceptionStatus.Active:
                return next == LicenseSeatExceptionStatus.Closed;
            case LicenseSeatExceptionStatus.Suspended:
                return next == LicenseSeatExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LicenseSeatExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"license seat exception"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt + Score, 2);
    }

    public LicenseSeatException Copy()
    {
        return new LicenseSeatException(Code, Name, ExpiresAt, Segment, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
