namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary warranty profile is in its life cycle.</summary>
public enum PrimaryWarrantyProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One primary warranty profile, with what the planners need to act on it.</summary>
public sealed class PrimaryWarrantyProfile(string code, string label, int leaveDays, string channel, int batchSize, string currency, double rating, double ratio, PrimaryWarrantyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public PrimaryWarrantyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary warranty profile in status open.</summary>
    public static PrimaryWarrantyProfile NewPrimaryWarrantyProfile(string code, string label)
    {
        return new PrimaryWarrantyProfile(code, label, 0, "", 0, "", 0.0, 0.0, PrimaryWarrantyProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryWarrantyProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryWarrantyProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryWarrantyProfileStatus.Open:
                switch (next)
                {
                    case PrimaryWarrantyProfileStatus.Resolved:
                    case PrimaryWarrantyProfileStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryWarrantyProfileStatus.InReview:
                switch (next)
                {
                    case PrimaryWarrantyProfileStatus.Resolved:
                    case PrimaryWarrantyProfileStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryWarrantyProfileStatus.Resolved:
                return next == PrimaryWarrantyProfileStatus.Archived;
            case PrimaryWarrantyProfileStatus.Reopened:
                switch (next)
                {
                    case PrimaryWarrantyProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryWarrantyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary warranty profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize - (double)LeaveDays, 2);
    }

    public PrimaryWarrantyProfile Copy()
    {
        return new PrimaryWarrantyProfile(
            Code,
            Label,
            LeaveDays,
            Channel,
            BatchSize,
            Currency,
            Rating,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
