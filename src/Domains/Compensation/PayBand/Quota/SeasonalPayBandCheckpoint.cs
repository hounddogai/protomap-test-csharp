namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay band checkpoint is in its life cycle.</summary>
public enum SeasonalPayBandCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal pay band checkpoint and its current status.</summary>
public sealed class SeasonalPayBandCheckpoint(string code, string label, double margin, string source, string country, int windowDays, SeasonalPayBandCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Source { get; set; } = source;
    public string Country { get; set; } = country;
    public int WindowDays { get; set; } = windowDays;
    public SeasonalPayBandCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay band checkpoint in status draft.</summary>
    public static SeasonalPayBandCheckpoint NewSeasonalPayBandCheckpoint(string code, string label)
    {
        return new SeasonalPayBandCheckpoint(code, label, 0.0, "", "", 0, SeasonalPayBandCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayBandCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayBandCheckpointStatus next)
    {
        switch (Status)
        {
            case SeasonalPayBandCheckpointStatus.Draft:
                return next == SeasonalPayBandCheckpointStatus.Suspended || next == SeasonalPayBandCheckpointStatus.Closed;
            case SeasonalPayBandCheckpointStatus.Pending:
                switch (next)
                {
                    case SeasonalPayBandCheckpointStatus.Active:
                    case SeasonalPayBandCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalPayBandCheckpointStatus.Active:
                return next == SeasonalPayBandCheckpointStatus.Suspended || next == SeasonalPayBandCheckpointStatus.Closed;
            case SeasonalPayBandCheckpointStatus.Suspended:
                return next == SeasonalPayBandCheckpointStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalPayBandCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay band checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays + Margin, 2);
    }

    public SeasonalPayBandCheckpoint Copy()
    {
        return new SeasonalPayBandCheckpoint(Code, Label, Margin, Source, Country, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
