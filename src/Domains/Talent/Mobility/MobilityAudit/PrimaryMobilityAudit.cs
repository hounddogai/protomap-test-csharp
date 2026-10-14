namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary mobility audit is in its life cycle.</summary>
public enum PrimaryMobilityAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary mobility audit as the mobility team keeps it.</summary>
public sealed class PrimaryMobilityAudit(string code, string title, bool archived, string team, double amount, PrimaryMobilityAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public PrimaryMobilityAuditStatus Status { get; set; } = status;

    /// <summary>Starts a primary mobility audit in status draft.</summary>
    public static PrimaryMobilityAudit NewPrimaryMobilityAudit(string code, string title)
    {
        return new PrimaryMobilityAudit(code, title, false, "", 0.0, PrimaryMobilityAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMobilityAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMobilityAuditStatus next)
    {
        switch (Status)
        {
            case PrimaryMobilityAuditStatus.Draft:
                switch (next)
                {
                    case PrimaryMobilityAuditStatus.Pending:
                    case PrimaryMobilityAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryMobilityAuditStatus.Pending:
                switch (next)
                {
                    case PrimaryMobilityAuditStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryMobilityAuditStatus.Active:
                switch (next)
                {
                    case PrimaryMobilityAuditStatus.Suspended:
                    case PrimaryMobilityAuditStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryMobilityAuditStatus.Suspended:
                switch (next)
                {
                    case PrimaryMobilityAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryMobilityAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary mobility audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PrimaryMobilityAudit Copy()
    {
        return new PrimaryMobilityAudit(Code, Title, Archived, Team, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
