namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination threshold is in its life cycle.</summary>
public enum NominationThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A nomination threshold. Its code is unique within a tenant.</summary>
public sealed class NominationThreshold(string code, string label, int endsAt, string skill, string segment, string location, double rating, int windowDays, NominationThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public NominationThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a nomination threshold in status idle.</summary>
    public static NominationThreshold NewNominationThreshold(string code, string label)
    {
        return new NominationThreshold(code, label, 0, "", "", "", 0.0, 0, NominationThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationThresholdStatus next)
    {
        switch (Status)
        {
            case NominationThresholdStatus.Idle:
                switch (next)
                {
                    case NominationThresholdStatus.Warming:
                        return true;
                }
                return false;
            case NominationThresholdStatus.Warming:
                return next == NominationThresholdStatus.Ready;
            case NominationThresholdStatus.Ready:
                return next == NominationThresholdStatus.Draining || next == NominationThresholdStatus.Stopped;
            case NominationThresholdStatus.Draining:
                return next == NominationThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(NominationThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rating - (double)EndsAt, 2);
    }

    public NominationThreshold Copy()
    {
        return new NominationThreshold(Code, Label, EndsAt, Skill, Segment, Location, Rating, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
