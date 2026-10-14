namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted garnishment variance is in its life cycle.</summary>
public enum WeightedGarnishmentVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One weighted garnishment variance, with what the planners need to act on it.</summary>
public sealed class WeightedGarnishmentVariance(string code, string title, string segment, int createdAt, int attempts, string name, int graceDays, int sequence, double fte, double rate, WeightedGarnishmentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public int CreatedAt { get; set; } = createdAt;
    public int Attempts { get; set; } = attempts;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public WeightedGarnishmentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted garnishment variance in status draft.</summary>
    public static WeightedGarnishmentVariance NewWeightedGarnishmentVariance(string code, string title)
    {
        return new WeightedGarnishmentVariance(
            code,
            title,
            "",
            0,
            0,
            "",
            0,
            0,
            0.0,
            0.0,
            WeightedGarnishmentVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedGarnishmentVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedGarnishmentVarianceStatus next)
    {
        switch (Status)
        {
            case WeightedGarnishmentVarianceStatus.Draft:
                return next == WeightedGarnishmentVarianceStatus.Active || next == WeightedGarnishmentVarianceStatus.Suspended;
            case WeightedGarnishmentVarianceStatus.Pending:
                return next == WeightedGarnishmentVarianceStatus.Suspended;
            case WeightedGarnishmentVarianceStatus.Active:
                switch (next)
                {
                    case WeightedGarnishmentVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedGarnishmentVarianceStatus.Suspended:
                return next == WeightedGarnishmentVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedGarnishmentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted garnishment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)CreatedAt - (double)Attempts, 2);
    }

    public WeightedGarnishmentVariance Copy()
    {
        return new WeightedGarnishmentVariance(
            Code,
            Title,
            Segment,
            CreatedAt,
            Attempts,
            Name,
            GraceDays,
            Sequence,
            Fte,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
