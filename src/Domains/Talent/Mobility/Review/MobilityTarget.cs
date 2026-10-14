namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility target is in its life cycle.</summary>
public enum MobilityTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a mobility target and its current status.</summary>
public sealed class MobilityTarget(string code, string name, string locale, bool approved, int retries, string ownerId, string label, MobilityTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public bool Approved { get; set; } = approved;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public string Label { get; set; } = label;
    public MobilityTargetStatus Status { get; set; } = status;

    /// <summary>Starts a mobility target in status new.</summary>
    public static MobilityTarget NewMobilityTarget(string code, string name)
    {
        return new MobilityTarget(code, name, "", false, 0, "", "", MobilityTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityTargetStatus next)
    {
        switch (Status)
        {
            case MobilityTargetStatus.New:
                switch (next)
                {
                    case MobilityTargetStatus.Queued:
                    case MobilityTargetStatus.Done:
                        return true;
                }
                return false;
            case MobilityTargetStatus.Queued:
                switch (next)
                {
                    case MobilityTargetStatus.Failed:
                        return true;
                }
                return false;
            case MobilityTargetStatus.Running:
                return next == MobilityTargetStatus.Failed;
            case MobilityTargetStatus.Done:
                switch (next)
                {
                    case MobilityTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"mobility target"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public MobilityTarget Copy()
    {
        return new MobilityTarget(Code, Name, Locale, Approved, Retries, OwnerId, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
