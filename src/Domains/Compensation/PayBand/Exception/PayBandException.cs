namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band exception is in its life cycle.</summary>
public enum PayBandExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pay band exception and its current status.</summary>
public sealed class PayBandException(string code, string name, double weight, int durationMinutes, int version, string summary, string skill, double floorValue, PayBandExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public PayBandExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pay band exception in status draft.</summary>
    public static PayBandException NewPayBandException(string code, string name)
    {
        return new PayBandException(code, name, 0.0, 0, 0, "", "", 0.0, PayBandExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandExceptionStatus next)
    {
        switch (Status)
        {
            case PayBandExceptionStatus.Draft:
                return next == PayBandExceptionStatus.Pending || next == PayBandExceptionStatus.Suspended;
            case PayBandExceptionStatus.Pending:
                switch (next)
                {
                    case PayBandExceptionStatus.Closed:
                        return true;
                }
                return false;
            case PayBandExceptionStatus.Active:
                switch (next)
                {
                    case PayBandExceptionStatus.Suspended:
                    case PayBandExceptionStatus.Closed:
                        return true;
                }
                return false;
            case PayBandExceptionStatus.Suspended:
                return next == PayBandExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayBandExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue * (double)DurationMinutes, 2);
    }

    public PayBandException Copy()
    {
        return new PayBandException(
            Code,
            Name,
            Weight,
            DurationMinutes,
            Version,
            Summary,
            Skill,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
