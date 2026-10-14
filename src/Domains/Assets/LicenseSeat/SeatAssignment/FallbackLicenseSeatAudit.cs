namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat audit is in its life cycle.</summary>
public enum FallbackLicenseSeatAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback license seat audit, with what the planners need to act on it.</summary>
public sealed class FallbackLicenseSeatAudit(string code, string name, int limit, int endsAt, double floorValue, string contactEmail, double hours, string country, int capacity, List<string> tags, double threshold, FallbackLicenseSeatAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public double Hours { get; set; } = hours;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public FallbackLicenseSeatAuditStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat audit in status draft.</summary>
    public static FallbackLicenseSeatAudit NewFallbackLicenseSeatAudit(string code, string name)
    {
        return new FallbackLicenseSeatAudit(
            code,
            name,
            0,
            0,
            0.0,
            "",
            0.0,
            "",
            0,
            new List<string>(),
            0.0,
            FallbackLicenseSeatAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatAuditStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatAuditStatus.Draft:
                switch (next)
                {
                    case FallbackLicenseSeatAuditStatus.Active:
                        return true;
                }
                return false;
            case FallbackLicenseSeatAuditStatus.Pending:
                switch (next)
                {
                    case FallbackLicenseSeatAuditStatus.Closed:
                        return true;
                }
                return false;
            case FallbackLicenseSeatAuditStatus.Active:
                switch (next)
                {
                    case FallbackLicenseSeatAuditStatus.Suspended:
                    case FallbackLicenseSeatAuditStatus.Closed:
                        return true;
                }
                return false;
            case FallbackLicenseSeatAuditStatus.Suspended:
                return next == FallbackLicenseSeatAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback license seat audit"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Capacity * Threshold, 2);
    }

    public FallbackLicenseSeatAudit Copy()
    {
        return new FallbackLicenseSeatAudit(
            Code,
            Name,
            Limit,
            EndsAt,
            FloorValue,
            ContactEmail,
            Hours,
            Country,
            Capacity,
            Tags,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
