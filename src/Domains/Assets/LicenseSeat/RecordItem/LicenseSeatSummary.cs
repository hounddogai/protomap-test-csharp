namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat summary is in its life cycle.</summary>
public enum LicenseSeatSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A license seat summary as the license seat team keeps it.</summary>
public sealed class LicenseSeatSummary(string code, string title, double discount, double cost, string notes, LicenseSeatSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public LicenseSeatSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a license seat summary in status requested.</summary>
    public static LicenseSeatSummary NewLicenseSeatSummary(string code, string title)
    {
        return new LicenseSeatSummary(code, title, 0.0, 0.0, "", LicenseSeatSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatSummaryStatus next)
    {
        switch (Status)
        {
            case LicenseSeatSummaryStatus.Requested:
                return next == LicenseSeatSummaryStatus.Granted || next == LicenseSeatSummaryStatus.Revoked;
            case LicenseSeatSummaryStatus.Granted:
                return next == LicenseSeatSummaryStatus.Revoked || next == LicenseSeatSummaryStatus.Expired;
            case LicenseSeatSummaryStatus.Revoked:
                switch (next)
                {
                    case LicenseSeatSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round(Discount * Cost, 2);
    }

    public LicenseSeatSummary Copy()
    {
        return new LicenseSeatSummary(Code, Title, Discount, Cost, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
