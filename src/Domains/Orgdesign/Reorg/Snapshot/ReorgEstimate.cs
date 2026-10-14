namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg estimate is in its life cycle.</summary>
public enum ReorgEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A reorg estimate. Its code is unique within a tenant.</summary>
public sealed class ReorgEstimate(string code, string label, bool active, string phone, string timezone, int updatedAt, int endsAt, int durationMinutes, ReorgEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ReorgEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a reorg estimate in status open.</summary>
    public static ReorgEstimate NewReorgEstimate(string code, string label)
    {
        return new ReorgEstimate(code, label, false, "", "", 0, 0, 0, ReorgEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgEstimateStatus next)
    {
        switch (Status)
        {
            case ReorgEstimateStatus.Open:
                return next == ReorgEstimateStatus.InReview || next == ReorgEstimateStatus.Reopened;
            case ReorgEstimateStatus.InReview:
                switch (next)
                {
                    case ReorgEstimateStatus.Resolved:
                    case ReorgEstimateStatus.Archived:
                        return true;
                }
                return false;
            case ReorgEstimateStatus.Resolved:
                return next == ReorgEstimateStatus.Reopened || next == ReorgEstimateStatus.Archived;
            case ReorgEstimateStatus.Reopened:
                switch (next)
                {
                    case ReorgEstimateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)DurationMinutes + (double)UpdatedAt, 2);
    }

    public ReorgEstimate Copy()
    {
        return new ReorgEstimate(Code, Label, Active, Phone, Timezone, UpdatedAt, EndsAt, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
