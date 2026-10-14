namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient retro pay rollup is in its life cycle.</summary>
public enum LenientRetroPayRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient retro pay rollup as the retro pay team keeps it.</summary>
public sealed class LenientRetroPayRollup(string code, string title, int updatedAt, double rate, int retries, string region, double fte, double maximum, string location, int batchSize, double amount, LenientRetroPayRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public double Amount { get; set; } = amount;
    public LenientRetroPayRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient retro pay rollup in status new.</summary>
    public static LenientRetroPayRollup NewLenientRetroPayRollup(string code, string title)
    {
        return new LenientRetroPayRollup(
            code,
            title,
            0,
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            0,
            0.0,
            LenientRetroPayRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRetroPayRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRetroPayRollupStatus next)
    {
        switch (Status)
        {
            case LenientRetroPayRollupStatus.New:
                return next == LenientRetroPayRollupStatus.Queued || next == LenientRetroPayRollupStatus.Done;
            case LenientRetroPayRollupStatus.Queued:
                switch (next)
                {
                    case LenientRetroPayRollupStatus.Running:
                    case LenientRetroPayRollupStatus.Failed:
                        return true;
                }
                return false;
            case LenientRetroPayRollupStatus.Running:
                return next == LenientRetroPayRollupStatus.Done || next == LenientRetroPayRollupStatus.Failed;
            case LenientRetroPayRollupStatus.Done:
                switch (next)
                {
                    case LenientRetroPayRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientRetroPayRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient retro pay rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Fte * Rate, 2);
    }

    public LenientRetroPayRollup Copy()
    {
        return new LenientRetroPayRollup(
            Code,
            Title,
            UpdatedAt,
            Rate,
            Retries,
            Region,
            Fte,
            Maximum,
            Location,
            BatchSize,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
