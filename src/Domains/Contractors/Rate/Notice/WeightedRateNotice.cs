namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate notice is in its life cycle.</summary>
public enum WeightedRateNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted rate notice and its current status.</summary>
public sealed class WeightedRateNotice(string code, string name, int windowDays, string checksum, int retries, string contactEmail, double weight, int graceDays, string title, bool active, WeightedRateNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public int Retries { get; set; } = retries;
    public string ContactEmail { get; set; } = contactEmail;
    public double Weight { get; set; } = weight;
    public int GraceDays { get; set; } = graceDays;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public WeightedRateNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate notice in status draft.</summary>
    public static WeightedRateNotice NewWeightedRateNotice(string code, string name)
    {
        return new WeightedRateNotice(code, name, 0, "", 0, "", 0.0, 0, "", false, WeightedRateNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRateNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRateNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedRateNoticeStatus.Draft:
                return next == WeightedRateNoticeStatus.Pending;
            case WeightedRateNoticeStatus.Pending:
                return next == WeightedRateNoticeStatus.Active || next == WeightedRateNoticeStatus.Closed;
            case WeightedRateNoticeStatus.Active:
                return next == WeightedRateNoticeStatus.Suspended || next == WeightedRateNoticeStatus.Closed;
            case WeightedRateNoticeStatus.Suspended:
                return next == WeightedRateNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedRateNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight - (double)WindowDays, 2);
    }

    public WeightedRateNotice Copy()
    {
        return new WeightedRateNotice(
            Code,
            Name,
            WindowDays,
            Checksum,
            Retries,
            ContactEmail,
            Weight,
            GraceDays,
            Title,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
