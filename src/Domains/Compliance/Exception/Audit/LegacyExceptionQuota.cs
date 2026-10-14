namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy exception quota is in its life cycle.</summary>
public enum LegacyExceptionQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy exception quota, with what the planners need to act on it.</summary>
public sealed class LegacyExceptionQuota(string code, string title, string employeeId, int expiresAt, string team, string department, int quantity, LegacyExceptionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public string Department { get; set; } = department;
    public int Quantity { get; set; } = quantity;
    public LegacyExceptionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a legacy exception quota in status draft.</summary>
    public static LegacyExceptionQuota NewLegacyExceptionQuota(string code, string title)
    {
        return new LegacyExceptionQuota(code, title, "", 0, "", "", 0, LegacyExceptionQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyExceptionQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyExceptionQuotaStatus next)
    {
        switch (Status)
        {
            case LegacyExceptionQuotaStatus.Draft:
                switch (next)
                {
                    case LegacyExceptionQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyExceptionQuotaStatus.Pending:
                return next == LegacyExceptionQuotaStatus.Active || next == LegacyExceptionQuotaStatus.Suspended;
            case LegacyExceptionQuotaStatus.Active:
                switch (next)
                {
                    case LegacyExceptionQuotaStatus.Suspended:
                    case LegacyExceptionQuotaStatus.Closed:
                        return true;
                }
                return false;
            case LegacyExceptionQuotaStatus.Suspended:
                return next == LegacyExceptionQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyExceptionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy exception quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt * (double)Quantity, 2);
    }

    public LegacyExceptionQuota Copy()
    {
        return new LegacyExceptionQuota(Code, Title, EmployeeId, ExpiresAt, Team, Department, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
