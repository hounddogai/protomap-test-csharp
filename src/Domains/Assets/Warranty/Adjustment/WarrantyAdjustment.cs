namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty adjustment is in its life cycle.</summary>
public enum WarrantyAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A warranty adjustment. Its code is unique within a tenant.</summary>
public sealed class WarrantyAdjustment(string code, string title, double amount, double rating, string managerId, int retries, double fte, string locale, WarrantyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public double Rating { get; set; } = rating;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public double Fte { get; set; } = fte;
    public string Locale { get; set; } = locale;
    public WarrantyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a warranty adjustment in status requested.</summary>
    public static WarrantyAdjustment NewWarrantyAdjustment(string code, string title)
    {
        return new WarrantyAdjustment(code, title, 0.0, 0.0, "", 0, 0.0, "", WarrantyAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyAdjustmentStatus next)
    {
        switch (Status)
        {
            case WarrantyAdjustmentStatus.Requested:
                return next == WarrantyAdjustmentStatus.Expired;
            case WarrantyAdjustmentStatus.Granted:
                return next == WarrantyAdjustmentStatus.Expired;
            case WarrantyAdjustmentStatus.Revoked:
                return next == WarrantyAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WarrantyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Amount * (double)Retries, 2);
    }

    public WarrantyAdjustment Copy()
    {
        return new WarrantyAdjustment(Code, Title, Amount, Rating, ManagerId, Retries, Fte, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
