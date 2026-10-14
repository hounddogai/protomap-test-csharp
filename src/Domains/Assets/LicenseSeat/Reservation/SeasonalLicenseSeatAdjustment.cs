namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal license seat adjustment is in its life cycle.</summary>
public enum SeasonalLicenseSeatAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal license seat adjustment. Its code is unique within a tenant.</summary>
public sealed class SeasonalLicenseSeatAdjustment(string code, string title, double margin, int attempts, string source, SeasonalLicenseSeatAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public string Source { get; set; } = source;
    public SeasonalLicenseSeatAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal license seat adjustment in status idle.</summary>
    public static SeasonalLicenseSeatAdjustment NewSeasonalLicenseSeatAdjustment(string code, string title)
    {
        return new SeasonalLicenseSeatAdjustment(code, title, 0.0, 0, "", SeasonalLicenseSeatAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLicenseSeatAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLicenseSeatAdjustmentStatus next)
    {
        switch (Status)
        {
            case SeasonalLicenseSeatAdjustmentStatus.Idle:
                switch (next)
                {
                    case SeasonalLicenseSeatAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatAdjustmentStatus.Warming:
                switch (next)
                {
                    case SeasonalLicenseSeatAdjustmentStatus.Ready:
                    case SeasonalLicenseSeatAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatAdjustmentStatus.Ready:
                return next == SeasonalLicenseSeatAdjustmentStatus.Stopped;
            case SeasonalLicenseSeatAdjustmentStatus.Draining:
                return next == SeasonalLicenseSeatAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalLicenseSeatAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal license seat adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts * Margin, 2);
    }

    public SeasonalLicenseSeatAdjustment Copy()
    {
        return new SeasonalLicenseSeatAdjustment(Code, Title, Margin, Attempts, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
