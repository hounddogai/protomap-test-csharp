namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive license seat notice is in its life cycle.</summary>
public enum AdaptiveLicenseSeatNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive license seat notice and its current status.</summary>
public sealed class AdaptiveLicenseSeatNotice(string code, string title, double ratio, double fte, int tenureMonths, int version, AdaptiveLicenseSeatNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public AdaptiveLicenseSeatNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive license seat notice in status requested.</summary>
    public static AdaptiveLicenseSeatNotice NewAdaptiveLicenseSeatNotice(string code, string title)
    {
        return new AdaptiveLicenseSeatNotice(code, title, 0.0, 0.0, 0, 0, AdaptiveLicenseSeatNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLicenseSeatNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatNoticeStatus.Requested:
                switch (next)
                {
                    case AdaptiveLicenseSeatNoticeStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveLicenseSeatNoticeStatus.Granted:
                switch (next)
                {
                    case AdaptiveLicenseSeatNoticeStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveLicenseSeatNoticeStatus.Revoked:
                switch (next)
                {
                    case AdaptiveLicenseSeatNoticeStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveLicenseSeatNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive license seat notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)TenureMonths * (double)Version, 2);
    }

    public AdaptiveLicenseSeatNotice Copy()
    {
        return new AdaptiveLicenseSeatNotice(Code, Title, Ratio, Fte, TenureMonths, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
