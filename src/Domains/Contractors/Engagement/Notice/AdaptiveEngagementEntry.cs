namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive engagement entry is in its life cycle.</summary>
public enum AdaptiveEngagementEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive engagement entry as the engagement team keeps it.</summary>
public sealed class AdaptiveEngagementEntry(string code, string label, string source, double rate, double ceiling, int revision, AdaptiveEngagementEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public AdaptiveEngagementEntryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive engagement entry in status requested.</summary>
    public static AdaptiveEngagementEntry NewAdaptiveEngagementEntry(string code, string label)
    {
        return new AdaptiveEngagementEntry(code, label, "", 0.0, 0.0, 0, AdaptiveEngagementEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEngagementEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEngagementEntryStatus next)
    {
        switch (Status)
        {
            case AdaptiveEngagementEntryStatus.Requested:
                switch (next)
                {
                    case AdaptiveEngagementEntryStatus.Granted:
                    case AdaptiveEngagementEntryStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveEngagementEntryStatus.Granted:
                switch (next)
                {
                    case AdaptiveEngagementEntryStatus.Revoked:
                    case AdaptiveEngagementEntryStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveEngagementEntryStatus.Revoked:
                return next == AdaptiveEngagementEntryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveEngagementEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive engagement entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive engagement entry"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Rate * Ceiling, 2);
    }

    public AdaptiveEngagementEntry Copy()
    {
        return new AdaptiveEngagementEntry(Code, Label, Source, Rate, Ceiling, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
