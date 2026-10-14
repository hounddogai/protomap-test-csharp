namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback nomination exception is in its life cycle.</summary>
public enum FallbackNominationExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback nomination exception as the nomination team keeps it.</summary>
public sealed class FallbackNominationException(string code, string name, string channel, int updatedAt, string badge, string department, FallbackNominationExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public FallbackNominationExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a fallback nomination exception in status created.</summary>
    public static FallbackNominationException NewFallbackNominationException(string code, string name)
    {
        return new FallbackNominationException(code, name, "", 0, "", "", FallbackNominationExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackNominationExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackNominationExceptionStatus next)
    {
        switch (Status)
        {
            case FallbackNominationExceptionStatus.Created:
                switch (next)
                {
                    case FallbackNominationExceptionStatus.Published:
                        return true;
                }
                return false;
            case FallbackNominationExceptionStatus.Validated:
                return next == FallbackNominationExceptionStatus.Retired;
            case FallbackNominationExceptionStatus.Published:
                return next == FallbackNominationExceptionStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackNominationExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback nomination exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public FallbackNominationException Copy()
    {
        return new FallbackNominationException(Code, Name, Channel, UpdatedAt, Badge, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
