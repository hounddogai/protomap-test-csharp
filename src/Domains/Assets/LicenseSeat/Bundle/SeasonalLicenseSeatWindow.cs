namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal license seat window is in its life cycle.</summary>
public enum SeasonalLicenseSeatWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal license seat window as the license seat team keeps it.</summary>
public sealed class SeasonalLicenseSeatWindow(string code, string title, string grade, double ratio, double floorValue, SeasonalLicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Ratio { get; set; } = ratio;
    public double FloorValue { get; set; } = floorValue;
    public SeasonalLicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal license seat window in status idle.</summary>
    public static SeasonalLicenseSeatWindow NewSeasonalLicenseSeatWindow(string code, string title)
    {
        return new SeasonalLicenseSeatWindow(code, title, "", 0.0, 0.0, SeasonalLicenseSeatWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLicenseSeatWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case SeasonalLicenseSeatWindowStatus.Idle:
                return next == SeasonalLicenseSeatWindowStatus.Warming || next == SeasonalLicenseSeatWindowStatus.Stopped;
            case SeasonalLicenseSeatWindowStatus.Warming:
                switch (next)
                {
                    case SeasonalLicenseSeatWindowStatus.Ready:
                    case SeasonalLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatWindowStatus.Ready:
                switch (next)
                {
                    case SeasonalLicenseSeatWindowStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatWindowStatus.Draining:
                return next == SeasonalLicenseSeatWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalLicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal license seat window"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue + Ratio, 2);
    }

    public SeasonalLicenseSeatWindow Copy()
    {
        return new SeasonalLicenseSeatWindow(Code, Title, Grade, Ratio, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
