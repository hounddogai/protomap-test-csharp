namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pay band quota is in its life cycle.</summary>
public enum StrictPayBandQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One strict pay band quota, with what the planners need to act on it.</summary>
public sealed class StrictPayBandQuota(string code, string label, double ceiling, int batchSize, int updatedAt, string source, string salaryBand, StrictPayBandQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public StrictPayBandQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict pay band quota in status open.</summary>
    public static StrictPayBandQuota NewStrictPayBandQuota(string code, string label)
    {
        return new StrictPayBandQuota(code, label, 0.0, 0, 0, "", "", StrictPayBandQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPayBandQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPayBandQuotaStatus next)
    {
        switch (Status)
        {
            case StrictPayBandQuotaStatus.Open:
                switch (next)
                {
                    case StrictPayBandQuotaStatus.Reopened:
                    case StrictPayBandQuotaStatus.Archived:
                        return true;
                }
                return false;
            case StrictPayBandQuotaStatus.InReview:
                return next == StrictPayBandQuotaStatus.Resolved || next == StrictPayBandQuotaStatus.Reopened;
            case StrictPayBandQuotaStatus.Resolved:
                switch (next)
                {
                    case StrictPayBandQuotaStatus.Reopened:
                    case StrictPayBandQuotaStatus.Archived:
                        return true;
                }
                return false;
            case StrictPayBandQuotaStatus.Reopened:
                return next == StrictPayBandQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictPayBandQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pay band quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"strict pay band quota"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ceiling - (double)UpdatedAt, 2);
    }

    public StrictPayBandQuota Copy()
    {
        return new StrictPayBandQuota(Code, Label, Ceiling, BatchSize, UpdatedAt, Source, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
