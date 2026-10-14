namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered license seat adjustment is in its life cycle.</summary>
public enum TieredLicenseSeatAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered license seat adjustment as the license seat team keeps it.</summary>
public sealed class TieredLicenseSeatAdjustment(string code, string title, bool active, double weight, double rate, double rating, string contactEmail, double margin, string summary, TieredLicenseSeatAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public double Rating { get; set; } = rating;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public string Summary { get; set; } = summary;
    public TieredLicenseSeatAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered license seat adjustment in status idle.</summary>
    public static TieredLicenseSeatAdjustment NewTieredLicenseSeatAdjustment(string code, string title)
    {
        return new TieredLicenseSeatAdjustment(
            code,
            title,
            false,
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            "",
            TieredLicenseSeatAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLicenseSeatAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLicenseSeatAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredLicenseSeatAdjustmentStatus.Idle:
                switch (next)
                {
                    case TieredLicenseSeatAdjustmentStatus.Ready:
                    case TieredLicenseSeatAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case TieredLicenseSeatAdjustmentStatus.Warming:
                return next == TieredLicenseSeatAdjustmentStatus.Ready || next == TieredLicenseSeatAdjustmentStatus.Draining;
            case TieredLicenseSeatAdjustmentStatus.Ready:
                switch (next)
                {
                    case TieredLicenseSeatAdjustmentStatus.Draining:
                    case TieredLicenseSeatAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case TieredLicenseSeatAdjustmentStatus.Draining:
                switch (next)
                {
                    case TieredLicenseSeatAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLicenseSeatAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered license seat adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered license seat adjustment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Rating * Weight, 2);
    }

    public TieredLicenseSeatAdjustment Copy()
    {
        return new TieredLicenseSeatAdjustment(
            Code,
            Title,
            Active,
            Weight,
            Rate,
            Rating,
            ContactEmail,
            Margin,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
