namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive holiday variance is in its life cycle.</summary>
public enum AdaptiveHolidayVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adaptive holiday variance and its current status.</summary>
public sealed class AdaptiveHolidayVariance(string code, string label, int windowDays, int retries, double margin, bool pinned, double amount, AdaptiveHolidayVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public double Margin { get; set; } = margin;
    public bool Pinned { get; set; } = pinned;
    public double Amount { get; set; } = amount;
    public AdaptiveHolidayVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive holiday variance in status new.</summary>
    public static AdaptiveHolidayVariance NewAdaptiveHolidayVariance(string code, string label)
    {
        return new AdaptiveHolidayVariance(code, label, 0, 0, 0.0, false, 0.0, AdaptiveHolidayVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHolidayVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHolidayVarianceStatus next)
    {
        switch (Status)
        {
            case AdaptiveHolidayVarianceStatus.New:
                return next == AdaptiveHolidayVarianceStatus.Running || next == AdaptiveHolidayVarianceStatus.Done;
            case AdaptiveHolidayVarianceStatus.Queued:
                return next == AdaptiveHolidayVarianceStatus.Running;
            case AdaptiveHolidayVarianceStatus.Running:
                switch (next)
                {
                    case AdaptiveHolidayVarianceStatus.Done:
                    case AdaptiveHolidayVarianceStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveHolidayVarianceStatus.Done:
                switch (next)
                {
                    case AdaptiveHolidayVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveHolidayVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive holiday variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive holiday variance"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Retries + Margin, 2);
    }

    public AdaptiveHolidayVariance Copy()
    {
        return new AdaptiveHolidayVariance(Code, Label, WindowDays, Retries, Margin, Pinned, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
