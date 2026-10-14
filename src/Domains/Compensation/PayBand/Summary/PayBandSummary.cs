namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band summary is in its life cycle.</summary>
public enum PayBandSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pay band summary and its current status.</summary>
public sealed class PayBandSummary(string code, string title, string category, double rate, int tenureMonths, string costCenter, string managerId, int revision, string jobCode, double ratio, PayBandSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public string JobCode { get; set; } = jobCode;
    public double Ratio { get; set; } = ratio;
    public PayBandSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pay band summary in status requested.</summary>
    public static PayBandSummary NewPayBandSummary(string code, string title)
    {
        return new PayBandSummary(code, title, "", 0.0, 0, "", "", 0, "", 0.0, PayBandSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandSummaryStatus next)
    {
        switch (Status)
        {
            case PayBandSummaryStatus.Requested:
                return next == PayBandSummaryStatus.Revoked;
            case PayBandSummaryStatus.Granted:
                switch (next)
                {
                    case PayBandSummaryStatus.Revoked:
                        return true;
                }
                return false;
            case PayBandSummaryStatus.Revoked:
                return next == PayBandSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayBandSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Ratio + (double)Revision, 2);
    }

    public PayBandSummary Copy()
    {
        return new PayBandSummary(
            Code,
            Title,
            Category,
            Rate,
            TenureMonths,
            CostCenter,
            ManagerId,
            Revision,
            JobCode,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
