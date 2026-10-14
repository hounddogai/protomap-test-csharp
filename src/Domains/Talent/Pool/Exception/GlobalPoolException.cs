namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pool exception is in its life cycle.</summary>
public enum GlobalPoolExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a global pool exception and its current status.</summary>
public sealed class GlobalPoolException(string code, string label, int endsAt, string timezone, string country, string contactEmail, GlobalPoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Timezone { get; set; } = timezone;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalPoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a global pool exception in status created.</summary>
    public static GlobalPoolException NewGlobalPoolException(string code, string label)
    {
        return new GlobalPoolException(code, label, 0, "", "", "", GlobalPoolExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPoolExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPoolExceptionStatus next)
    {
        switch (Status)
        {
            case GlobalPoolExceptionStatus.Created:
                switch (next)
                {
                    case GlobalPoolExceptionStatus.Validated:
                    case GlobalPoolExceptionStatus.Published:
                        return true;
                }
                return false;
            case GlobalPoolExceptionStatus.Validated:
                switch (next)
                {
                    case GlobalPoolExceptionStatus.Retired:
                        return true;
                }
                return false;
            case GlobalPoolExceptionStatus.Published:
                return next == GlobalPoolExceptionStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalPoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public GlobalPoolException Copy()
    {
        return new GlobalPoolException(Code, Label, EndsAt, Timezone, Country, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
