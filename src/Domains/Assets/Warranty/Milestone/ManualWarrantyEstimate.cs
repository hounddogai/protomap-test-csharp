namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual warranty estimate is in its life cycle.</summary>
public enum ManualWarrantyEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual warranty estimate, with what the planners need to act on it.</summary>
public sealed class ManualWarrantyEstimate(string code, string label, string locale, int sequence, string notes, string title, int startsAt, bool pinned, int limit, string phone, bool archived, ManualWarrantyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public ManualWarrantyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a manual warranty estimate in status idle.</summary>
    public static ManualWarrantyEstimate NewManualWarrantyEstimate(string code, string label)
    {
        return new ManualWarrantyEstimate(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            false,
            0,
            "",
            false,
            ManualWarrantyEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualWarrantyEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualWarrantyEstimateStatus next)
    {
        switch (Status)
        {
            case ManualWarrantyEstimateStatus.Idle:
                return next == ManualWarrantyEstimateStatus.Warming || next == ManualWarrantyEstimateStatus.Ready;
            case ManualWarrantyEstimateStatus.Warming:
                switch (next)
                {
                    case ManualWarrantyEstimateStatus.Ready:
                    case ManualWarrantyEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case ManualWarrantyEstimateStatus.Ready:
                return next == ManualWarrantyEstimateStatus.Draining;
            case ManualWarrantyEstimateStatus.Draining:
                return next == ManualWarrantyEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualWarrantyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual warranty estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Limit + (double)Sequence, 2);
    }

    public ManualWarrantyEstimate Copy()
    {
        return new ManualWarrantyEstimate(
            Code,
            Label,
            Locale,
            Sequence,
            Notes,
            Title,
            StartsAt,
            Pinned,
            Limit,
            Phone,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
