namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped training due digest is in its life cycle.</summary>
public enum CappedTrainingDueDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A capped training due digest. Its code is unique within a tenant.</summary>
public sealed class CappedTrainingDueDigest(string code, string name, int startsAt, bool approved, bool pinned, string reviewer, string team, string costCenter, CappedTrainingDueDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public bool Approved { get; set; } = approved;
    public bool Pinned { get; set; } = pinned;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public CappedTrainingDueDigestStatus Status { get; set; } = status;

    /// <summary>Starts a capped training due digest in status proposed.</summary>
    public static CappedTrainingDueDigest NewCappedTrainingDueDigest(string code, string name)
    {
        return new CappedTrainingDueDigest(
            code,
            name,
            0,
            false,
            false,
            "",
            "",
            "",
            CappedTrainingDueDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTrainingDueDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTrainingDueDigestStatus next)
    {
        switch (Status)
        {
            case CappedTrainingDueDigestStatus.Proposed:
                return next == CappedTrainingDueDigestStatus.Rejected || next == CappedTrainingDueDigestStatus.Withdrawn;
            case CappedTrainingDueDigestStatus.Approved:
                switch (next)
                {
                    case CappedTrainingDueDigestStatus.Rejected:
                    case CappedTrainingDueDigestStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedTrainingDueDigestStatus.Rejected:
                switch (next)
                {
                    case CappedTrainingDueDigestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedTrainingDueDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped training due digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public CappedTrainingDueDigest Copy()
    {
        return new CappedTrainingDueDigest(
            Code,
            Name,
            StartsAt,
            Approved,
            Pinned,
            Reviewer,
            Team,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
