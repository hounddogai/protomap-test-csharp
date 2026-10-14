namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling training due quota is in its life cycle.</summary>
public enum RollingTrainingDueQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling training due quota as the training due team keeps it.</summary>
public sealed class RollingTrainingDueQuota(string code, string name, string currency, double floorValue, int endsAt, bool visible, string country, bool active, bool pinned, string phone, RollingTrainingDueQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public RollingTrainingDueQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a rolling training due quota in status open.</summary>
    public static RollingTrainingDueQuota NewRollingTrainingDueQuota(string code, string name)
    {
        return new RollingTrainingDueQuota(
            code,
            name,
            "",
            0.0,
            0,
            false,
            "",
            false,
            false,
            "",
            RollingTrainingDueQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingTrainingDueQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingTrainingDueQuotaStatus next)
    {
        switch (Status)
        {
            case RollingTrainingDueQuotaStatus.Open:
                return next == RollingTrainingDueQuotaStatus.InReview || next == RollingTrainingDueQuotaStatus.Archived;
            case RollingTrainingDueQuotaStatus.InReview:
                switch (next)
                {
                    case RollingTrainingDueQuotaStatus.Resolved:
                        return true;
                }
                return false;
            case RollingTrainingDueQuotaStatus.Resolved:
                return next == RollingTrainingDueQuotaStatus.Reopened || next == RollingTrainingDueQuotaStatus.Archived;
            case RollingTrainingDueQuotaStatus.Reopened:
                switch (next)
                {
                    case RollingTrainingDueQuotaStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingTrainingDueQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling training due quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue + (double)EndsAt, 2);
    }

    public RollingTrainingDueQuota Copy()
    {
        return new RollingTrainingDueQuota(
            Code,
            Name,
            Currency,
            FloorValue,
            EndsAt,
            Visible,
            Country,
            Active,
            Pinned,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
