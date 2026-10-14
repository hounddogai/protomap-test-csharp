namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered nomination audit is in its life cycle.</summary>
public enum TieredNominationAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered nomination audit. Its code is unique within a tenant.</summary>
public sealed class TieredNominationAudit(string code, string title, bool archived, double floorValue, string phone, int retries, int revision, double minimum, int version, bool pinned, double ratio, TieredNominationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public int Revision { get; set; } = revision;
    public double Minimum { get; set; } = minimum;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public TieredNominationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a tiered nomination audit in status proposed.</summary>
    public static TieredNominationAudit NewTieredNominationAudit(string code, string title)
    {
        return new TieredNominationAudit(
            code,
            title,
            false,
            0.0,
            "",
            0,
            0,
            0.0,
            0,
            false,
            0.0,
            TieredNominationAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredNominationAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredNominationAuditStatus next)
    {
        switch (Status)
        {
            case TieredNominationAuditStatus.Proposed:
                switch (next)
                {
                    case TieredNominationAuditStatus.Approved:
                        return true;
                }
                return false;
            case TieredNominationAuditStatus.Approved:
                switch (next)
                {
                    case TieredNominationAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredNominationAuditStatus.Rejected:
                switch (next)
                {
                    case TieredNominationAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredNominationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered nomination audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Retries * Minimum, 2);
    }

    public TieredNominationAudit Copy()
    {
        return new TieredNominationAudit(
            Code,
            Title,
            Archived,
            FloorValue,
            Phone,
            Retries,
            Revision,
            Minimum,
            Version,
            Pinned,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
