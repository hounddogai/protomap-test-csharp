namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall digest is in its life cycle.</summary>
public enum TownHallDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A town hall digest. Its code is unique within a tenant.</summary>
public sealed class TownHallDigest(string code, string label, string managerId, string grade, double score, int version, double margin, string costCenter, List<string> tags, int endsAt, TownHallDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string Grade { get; set; } = grade;
    public double Score { get; set; } = score;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public int EndsAt { get; set; } = endsAt;
    public TownHallDigestStatus Status { get; set; } = status;

    /// <summary>Starts a town hall digest in status proposed.</summary>
    public static TownHallDigest NewTownHallDigest(string code, string label)
    {
        return new TownHallDigest(
            code,
            label,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            new List<string>(),
            0,
            TownHallDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallDigestStatus next)
    {
        switch (Status)
        {
            case TownHallDigestStatus.Proposed:
                return next == TownHallDigestStatus.Approved || next == TownHallDigestStatus.Withdrawn;
            case TownHallDigestStatus.Approved:
                switch (next)
                {
                    case TownHallDigestStatus.Rejected:
                    case TownHallDigestStatus.Withdrawn:
                        return true;
                }
                return false;
            case TownHallDigestStatus.Rejected:
                switch (next)
                {
                    case TownHallDigestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Margin * (double)Version, 2);
    }

    public TownHallDigest Copy()
    {
        return new TownHallDigest(
            Code,
            Label,
            ManagerId,
            Grade,
            Score,
            Version,
            Margin,
            CostCenter,
            Tags,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
