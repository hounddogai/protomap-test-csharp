namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback feedback reservation is in its life cycle.</summary>
public enum FallbackFeedbackReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback feedback reservation. Its code is unique within a tenant.</summary>
public sealed class FallbackFeedbackReservation(string code, string label, int windowDays, double score, string checksum, string name, int version, string notes, Dictionary<string, string> attributes, int createdAt, string badge, FallbackFeedbackReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public string Checksum { get; set; } = checksum;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public FallbackFeedbackReservationStatus Status { get; set; } = status;

    /// <summary>Starts a fallback feedback reservation in status proposed.</summary>
    public static FallbackFeedbackReservation NewFallbackFeedbackReservation(string code, string label)
    {
        return new FallbackFeedbackReservation(
            code,
            label,
            0,
            0.0,
            "",
            "",
            0,
            "",
            new Dictionary<string, string>(),
            0,
            "",
            FallbackFeedbackReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackFeedbackReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackFeedbackReservationStatus next)
    {
        switch (Status)
        {
            case FallbackFeedbackReservationStatus.Proposed:
                return next == FallbackFeedbackReservationStatus.Approved || next == FallbackFeedbackReservationStatus.Withdrawn;
            case FallbackFeedbackReservationStatus.Approved:
                return next == FallbackFeedbackReservationStatus.Withdrawn;
            case FallbackFeedbackReservationStatus.Rejected:
                return next == FallbackFeedbackReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackFeedbackReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback feedback reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays * (double)CreatedAt, 2);
    }

    public FallbackFeedbackReservation Copy()
    {
        return new FallbackFeedbackReservation(
            Code,
            Label,
            WindowDays,
            Score,
            Checksum,
            Name,
            Version,
            Notes,
            Attributes,
            CreatedAt,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
