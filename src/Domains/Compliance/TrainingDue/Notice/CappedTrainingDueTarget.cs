namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped training due target is in its life cycle.</summary>
public enum CappedTrainingDueTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped training due target, with what the planners need to act on it.</summary>
public sealed class CappedTrainingDueTarget(string code, string label, int graceDays, string checksum, int leaveDays, Dictionary<string, string> attributes, CappedTrainingDueTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string Checksum { get; set; } = checksum;
    public int LeaveDays { get; set; } = leaveDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CappedTrainingDueTargetStatus Status { get; set; } = status;

    /// <summary>Starts a capped training due target in status idle.</summary>
    public static CappedTrainingDueTarget NewCappedTrainingDueTarget(string code, string label)
    {
        return new CappedTrainingDueTarget(
            code,
            label,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            CappedTrainingDueTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTrainingDueTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTrainingDueTargetStatus next)
    {
        switch (Status)
        {
            case CappedTrainingDueTargetStatus.Idle:
                return next == CappedTrainingDueTargetStatus.Ready;
            case CappedTrainingDueTargetStatus.Warming:
                switch (next)
                {
                    case CappedTrainingDueTargetStatus.Stopped:
                        return true;
                }
                return false;
            case CappedTrainingDueTargetStatus.Ready:
                switch (next)
                {
                    case CappedTrainingDueTargetStatus.Draining:
                        return true;
                }
                return false;
            case CappedTrainingDueTargetStatus.Draining:
                switch (next)
                {
                    case CappedTrainingDueTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedTrainingDueTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped training due target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped training due target"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)GraceDays * (double)LeaveDays, 2);
    }

    public CappedTrainingDueTarget Copy()
    {
        return new CappedTrainingDueTarget(Code, Label, GraceDays, Checksum, LeaveDays, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
