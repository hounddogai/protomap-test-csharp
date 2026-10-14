namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay metric is in its life cycle.</summary>
public enum RetroPayMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a retro pay metric and its current status.</summary>
public sealed class RetroPayMetric(string code, string title, double fte, double ratio, double maximum, int retries, double minimum, string location, string description, double score, string reviewer, RetroPayMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public double Ratio { get; set; } = ratio;
    public double Maximum { get; set; } = maximum;
    public int Retries { get; set; } = retries;
    public double Minimum { get; set; } = minimum;
    public string Location { get; set; } = location;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public RetroPayMetricStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay metric in status proposed.</summary>
    public static RetroPayMetric NewRetroPayMetric(string code, string title)
    {
        return new RetroPayMetric(
            code,
            title,
            0.0,
            0.0,
            0.0,
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            RetroPayMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayMetricStatus next)
    {
        switch (Status)
        {
            case RetroPayMetricStatus.Proposed:
                return next == RetroPayMetricStatus.Rejected || next == RetroPayMetricStatus.Withdrawn;
            case RetroPayMetricStatus.Approved:
                switch (next)
                {
                    case RetroPayMetricStatus.Rejected:
                    case RetroPayMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case RetroPayMetricStatus.Rejected:
                return next == RetroPayMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RetroPayMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay metric"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum - (double)Retries, 2);
    }

    public RetroPayMetric Copy()
    {
        return new RetroPayMetric(
            Code,
            Title,
            Fte,
            Ratio,
            Maximum,
            Retries,
            Minimum,
            Location,
            Description,
            Score,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
