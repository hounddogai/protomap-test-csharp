namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle quota is in its life cycle.</summary>
public enum MeritCycleQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A merit cycle quota. Its code is unique within a tenant.</summary>
public sealed class MeritCycleQuota(string code, string title, double score, string category, bool approved, string currency, string location, double floorValue, string notes, string summary, int updatedAt, MeritCycleQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public string Location { get; set; } = location;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public int UpdatedAt { get; set; } = updatedAt;
    public MeritCycleQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle quota in status draft.</summary>
    public static MeritCycleQuota NewMeritCycleQuota(string code, string title)
    {
        return new MeritCycleQuota(code, title, 0.0, "", false, "", "", 0.0, "", "", 0, MeritCycleQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleQuotaStatus next)
    {
        switch (Status)
        {
            case MeritCycleQuotaStatus.Draft:
                switch (next)
                {
                    case MeritCycleQuotaStatus.Active:
                        return true;
                }
                return false;
            case MeritCycleQuotaStatus.Pending:
                return next == MeritCycleQuotaStatus.Closed;
            case MeritCycleQuotaStatus.Active:
                switch (next)
                {
                    case MeritCycleQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case MeritCycleQuotaStatus.Suspended:
                switch (next)
                {
                    case MeritCycleQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt * FloorValue, 2);
    }

    public MeritCycleQuota Copy()
    {
        return new MeritCycleQuota(
            Code,
            Title,
            Score,
            Category,
            Approved,
            Currency,
            Location,
            FloorValue,
            Notes,
            Summary,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
