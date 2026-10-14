namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched mobility profile is in its life cycle.</summary>
public enum BatchedMobilityProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched mobility profile. Its code is unique within a tenant.</summary>
public sealed class BatchedMobilityProfile(string code, string label, int tenureMonths, bool active, string description, string source, int updatedAt, string skill, string costCenter, string managerId, BatchedMobilityProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public string Description { get; set; } = description;
    public string Source { get; set; } = source;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public BatchedMobilityProfileStatus Status { get; set; } = status;

    /// <summary>Starts a batched mobility profile in status open.</summary>
    public static BatchedMobilityProfile NewBatchedMobilityProfile(string code, string label)
    {
        return new BatchedMobilityProfile(
            code,
            label,
            0,
            false,
            "",
            "",
            0,
            "",
            "",
            "",
            BatchedMobilityProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMobilityProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMobilityProfileStatus next)
    {
        switch (Status)
        {
            case BatchedMobilityProfileStatus.Open:
                switch (next)
                {
                    case BatchedMobilityProfileStatus.InReview:
                    case BatchedMobilityProfileStatus.Archived:
                        return true;
                }
                return false;
            case BatchedMobilityProfileStatus.InReview:
                switch (next)
                {
                    case BatchedMobilityProfileStatus.Reopened:
                    case BatchedMobilityProfileStatus.Archived:
                        return true;
                }
                return false;
            case BatchedMobilityProfileStatus.Resolved:
                return next == BatchedMobilityProfileStatus.Reopened || next == BatchedMobilityProfileStatus.Archived;
            case BatchedMobilityProfileStatus.Reopened:
                switch (next)
                {
                    case BatchedMobilityProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedMobilityProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched mobility profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)UpdatedAt * (double)TenureMonths, 2);
    }

    public BatchedMobilityProfile Copy()
    {
        return new BatchedMobilityProfile(
            Code,
            Label,
            TenureMonths,
            Active,
            Description,
            Source,
            UpdatedAt,
            Skill,
            CostCenter,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
