namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient retro pay summary is in its life cycle.</summary>
public enum LenientRetroPaySummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a lenient retro pay summary and its current status.</summary>
public sealed class LenientRetroPaySummary(string code, string name, string country, int quantity, int endsAt, string summary, string costCenter, double fte, string checksum, LenientRetroPaySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Quantity { get; set; } = quantity;
    public int EndsAt { get; set; } = endsAt;
    public string Summary { get; set; } = summary;
    public string CostCenter { get; set; } = costCenter;
    public double Fte { get; set; } = fte;
    public string Checksum { get; set; } = checksum;
    public LenientRetroPaySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient retro pay summary in status requested.</summary>
    public static LenientRetroPaySummary NewLenientRetroPaySummary(string code, string name)
    {
        return new LenientRetroPaySummary(
            code,
            name,
            "",
            0,
            0,
            "",
            "",
            0.0,
            "",
            LenientRetroPaySummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRetroPaySummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRetroPaySummaryStatus next)
    {
        switch (Status)
        {
            case LenientRetroPaySummaryStatus.Requested:
                return next == LenientRetroPaySummaryStatus.Granted || next == LenientRetroPaySummaryStatus.Expired;
            case LenientRetroPaySummaryStatus.Granted:
                switch (next)
                {
                    case LenientRetroPaySummaryStatus.Revoked:
                    case LenientRetroPaySummaryStatus.Expired:
                        return true;
                }
                return false;
            case LenientRetroPaySummaryStatus.Revoked:
                return next == LenientRetroPaySummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientRetroPaySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient retro pay summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity + Fte, 2);
    }

    public LenientRetroPaySummary Copy()
    {
        return new LenientRetroPaySummary(
            Code,
            Name,
            Country,
            Quantity,
            EndsAt,
            Summary,
            CostCenter,
            Fte,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
