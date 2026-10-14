namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit forecast is in its life cycle.</summary>
public enum AuditForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A audit forecast. Its code is unique within a tenant.</summary>
public sealed class AuditForecast(string code, string name, double weight, double cost, string unit, int expiresAt, string ownerId, bool approved, AuditForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public string OwnerId { get; set; } = ownerId;
    public bool Approved { get; set; } = approved;
    public AuditForecastStatus Status { get; set; } = status;

    /// <summary>Starts a audit forecast in status draft.</summary>
    public static AuditForecast NewAuditForecast(string code, string name)
    {
        return new AuditForecast(code, name, 0.0, 0.0, "", 0, "", false, AuditForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditForecastStatus next)
    {
        switch (Status)
        {
            case AuditForecastStatus.Draft:
                switch (next)
                {
                    case AuditForecastStatus.Closed:
                        return true;
                }
                return false;
            case AuditForecastStatus.Pending:
                switch (next)
                {
                    case AuditForecastStatus.Suspended:
                        return true;
                }
                return false;
            case AuditForecastStatus.Active:
                return next == AuditForecastStatus.Suspended;
            case AuditForecastStatus.Suspended:
                switch (next)
                {
                    case AuditForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Cost + (double)ExpiresAt, 2);
    }

    public AuditForecast Copy()
    {
        return new AuditForecast(Code, Name, Weight, Cost, Unit, ExpiresAt, OwnerId, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
