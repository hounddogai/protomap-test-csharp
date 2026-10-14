namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered holiday review is in its life cycle.</summary>
public enum TieredHolidayReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered holiday review. Its code is unique within a tenant.</summary>
public sealed class TieredHolidayReview(string code, string name, string source, int windowDays, bool pinned, int tenureMonths, double amount, bool approved, string team, TieredHolidayReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public bool Approved { get; set; } = approved;
    public string Team { get; set; } = team;
    public TieredHolidayReviewStatus Status { get; set; } = status;

    /// <summary>Starts a tiered holiday review in status requested.</summary>
    public static TieredHolidayReview NewTieredHolidayReview(string code, string name)
    {
        return new TieredHolidayReview(
            code,
            name,
            "",
            0,
            false,
            0,
            0.0,
            false,
            "",
            TieredHolidayReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredHolidayReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredHolidayReviewStatus next)
    {
        switch (Status)
        {
            case TieredHolidayReviewStatus.Requested:
                return next == TieredHolidayReviewStatus.Granted || next == TieredHolidayReviewStatus.Expired;
            case TieredHolidayReviewStatus.Granted:
                return next == TieredHolidayReviewStatus.Revoked || next == TieredHolidayReviewStatus.Expired;
            case TieredHolidayReviewStatus.Revoked:
                return next == TieredHolidayReviewStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredHolidayReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered holiday review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered holiday review"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays * (double)TenureMonths, 2);
    }

    public TieredHolidayReview Copy()
    {
        return new TieredHolidayReview(
            Code,
            Name,
            Source,
            WindowDays,
            Pinned,
            TenureMonths,
            Amount,
            Approved,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
