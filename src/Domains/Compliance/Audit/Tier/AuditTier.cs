namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit tier is in its life cycle.</summary>
public enum AuditTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One audit tier, with what the planners need to act on it.</summary>
public sealed class AuditTier(string code, string title, string grade, double cost, int durationMinutes, string team, int sequence, string phone, string checksum, AuditTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Team { get; set; } = team;
    public int Sequence { get; set; } = sequence;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public AuditTierStatus Status { get; set; } = status;

    /// <summary>Starts a audit tier in status draft.</summary>
    public static AuditTier NewAuditTier(string code, string title)
    {
        return new AuditTier(code, title, "", 0.0, 0, "", 0, "", "", AuditTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditTierStatus next)
    {
        switch (Status)
        {
            case AuditTierStatus.Draft:
                switch (next)
                {
                    case AuditTierStatus.Suspended:
                    case AuditTierStatus.Closed:
                        return true;
                }
                return false;
            case AuditTierStatus.Pending:
                return next == AuditTierStatus.Active || next == AuditTierStatus.Closed;
            case AuditTierStatus.Active:
                return next == AuditTierStatus.Suspended;
            case AuditTierStatus.Suspended:
                switch (next)
                {
                    case AuditTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Sequence - Cost, 2);
    }

    public AuditTier Copy()
    {
        return new AuditTier(Code, Title, Grade, Cost, DurationMinutes, Team, Sequence, Phone, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
