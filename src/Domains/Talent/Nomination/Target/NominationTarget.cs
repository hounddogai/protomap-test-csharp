namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination target is in its life cycle.</summary>
public enum NominationTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One nomination target, with what the planners need to act on it.</summary>
public sealed class NominationTarget(string code, string name, string title, string checksum, double weight, double threshold, double minimum, NominationTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public NominationTargetStatus Status { get; set; } = status;

    /// <summary>Starts a nomination target in status planned.</summary>
    public static NominationTarget NewNominationTarget(string code, string name)
    {
        return new NominationTarget(code, name, "", "", 0.0, 0.0, 0.0, NominationTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationTargetStatus next)
    {
        switch (Status)
        {
            case NominationTargetStatus.Planned:
                return next == NominationTargetStatus.Scheduled;
            case NominationTargetStatus.Scheduled:
                switch (next)
                {
                    case NominationTargetStatus.InProgress:
                    case NominationTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case NominationTargetStatus.InProgress:
                return next == NominationTargetStatus.Completed || next == NominationTargetStatus.Cancelled;
            case NominationTargetStatus.Completed:
                switch (next)
                {
                    case NominationTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"nomination target"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Threshold * Minimum, 2);
    }

    public NominationTarget Copy()
    {
        return new NominationTarget(Code, Name, Title, Checksum, Weight, Threshold, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
