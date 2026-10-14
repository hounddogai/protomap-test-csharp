namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall adjustment is in its life cycle.</summary>
public enum TownHallAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A town hall adjustment. Its code is unique within a tenant.</summary>
public sealed class TownHallAdjustment(string code, string label, int windowDays, string externalRef, double floorValue, int updatedAt, int attempts, string salaryBand, double hours, string contactEmail, int retries, TownHallAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public int Retries { get; set; } = retries;
    public TownHallAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a town hall adjustment in status open.</summary>
    public static TownHallAdjustment NewTownHallAdjustment(string code, string label)
    {
        return new TownHallAdjustment(code, label, 0, "", 0.0, 0, 0, "", 0.0, "", 0, TownHallAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallAdjustmentStatus next)
    {
        switch (Status)
        {
            case TownHallAdjustmentStatus.Open:
                return next == TownHallAdjustmentStatus.InReview;
            case TownHallAdjustmentStatus.InReview:
                switch (next)
                {
                    case TownHallAdjustmentStatus.Resolved:
                        return true;
                }
                return false;
            case TownHallAdjustmentStatus.Resolved:
                return next == TownHallAdjustmentStatus.Archived;
            case TownHallAdjustmentStatus.Reopened:
                switch (next)
                {
                    case TownHallAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Hours + (double)Attempts, 2);
    }

    public TownHallAdjustment Copy()
    {
        return new TownHallAdjustment(
            Code,
            Label,
            WindowDays,
            ExternalRef,
            FloorValue,
            UpdatedAt,
            Attempts,
            SalaryBand,
            Hours,
            ContactEmail,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
