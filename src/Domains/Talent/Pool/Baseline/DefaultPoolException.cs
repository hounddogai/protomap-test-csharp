namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pool exception is in its life cycle.</summary>
public enum DefaultPoolExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a default pool exception and its current status.</summary>
public sealed class DefaultPoolException(string code, string title, string location, bool approved, double rating, int attempts, string segment, double rate, double ceiling, string contactEmail, int revision, DefaultPoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public bool Approved { get; set; } = approved;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public string ContactEmail { get; set; } = contactEmail;
    public int Revision { get; set; } = revision;
    public DefaultPoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a default pool exception in status created.</summary>
    public static DefaultPoolException NewDefaultPoolException(string code, string title)
    {
        return new DefaultPoolException(
            code,
            title,
            "",
            false,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            0,
            DefaultPoolExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPoolExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPoolExceptionStatus next)
    {
        switch (Status)
        {
            case DefaultPoolExceptionStatus.Created:
                switch (next)
                {
                    case DefaultPoolExceptionStatus.Published:
                        return true;
                }
                return false;
            case DefaultPoolExceptionStatus.Validated:
                switch (next)
                {
                    case DefaultPoolExceptionStatus.Published:
                    case DefaultPoolExceptionStatus.Retired:
                        return true;
                }
                return false;
            case DefaultPoolExceptionStatus.Published:
                switch (next)
                {
                    case DefaultPoolExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)Attempts + Rate, 2);
    }

    public DefaultPoolException Copy()
    {
        return new DefaultPoolException(
            Code,
            Title,
            Location,
            Approved,
            Rating,
            Attempts,
            Segment,
            Rate,
            Ceiling,
            ContactEmail,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
