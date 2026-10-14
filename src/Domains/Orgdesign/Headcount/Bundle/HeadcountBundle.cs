namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount bundle is in its life cycle.</summary>
public enum HeadcountBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a headcount bundle and its current status.</summary>
public sealed class HeadcountBundle(string code, string label, double ceiling, int graceDays, int createdAt, double rating, HeadcountBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public double Rating { get; set; } = rating;
    public HeadcountBundleStatus Status { get; set; } = status;

    /// <summary>Starts a headcount bundle in status planned.</summary>
    public static HeadcountBundle NewHeadcountBundle(string code, string label)
    {
        return new HeadcountBundle(code, label, 0.0, 0, 0, 0.0, HeadcountBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountBundleStatus next)
    {
        switch (Status)
        {
            case HeadcountBundleStatus.Planned:
                switch (next)
                {
                    case HeadcountBundleStatus.InProgress:
                    case HeadcountBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountBundleStatus.Scheduled:
                switch (next)
                {
                    case HeadcountBundleStatus.InProgress:
                    case HeadcountBundleStatus.Completed:
                        return true;
                }
                return false;
            case HeadcountBundleStatus.InProgress:
                switch (next)
                {
                    case HeadcountBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountBundleStatus.Completed:
                return next == HeadcountBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(HeadcountBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ceiling - Rating, 2);
    }

    public HeadcountBundle Copy()
    {
        return new HeadcountBundle(Code, Label, Ceiling, GraceDays, CreatedAt, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
