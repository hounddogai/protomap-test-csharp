namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit request is in its life cycle.</summary>
public enum AuditRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One audit request, with what the planners need to act on it.</summary>
public sealed class AuditRequest(string code, string label, string location, int updatedAt, string currency, double budget, string externalRef, AuditRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public AuditRequestStatus Status { get; set; } = status;

    /// <summary>Starts a audit request in status draft.</summary>
    public static AuditRequest NewAuditRequest(string code, string label)
    {
        return new AuditRequest(code, label, "", 0, "", 0.0, "", AuditRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditRequestStatus next)
    {
        switch (Status)
        {
            case AuditRequestStatus.Draft:
                switch (next)
                {
                    case AuditRequestStatus.Closed:
                        return true;
                }
                return false;
            case AuditRequestStatus.Pending:
                return next == AuditRequestStatus.Closed;
            case AuditRequestStatus.Active:
                return next == AuditRequestStatus.Suspended;
            case AuditRequestStatus.Suspended:
                switch (next)
                {
                    case AuditRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)UpdatedAt - Budget, 2);
    }

    public AuditRequest Copy()
    {
        return new AuditRequest(Code, Label, Location, UpdatedAt, Currency, Budget, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
