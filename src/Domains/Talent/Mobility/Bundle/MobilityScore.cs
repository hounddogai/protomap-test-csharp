namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility score is in its life cycle.</summary>
public enum MobilityScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One mobility score, with what the planners need to act on it.</summary>
public sealed class MobilityScore(string code, string title, int limit, int version, int batchSize, MobilityScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public MobilityScoreStatus Status { get; set; } = status;

    /// <summary>Starts a mobility score in status new.</summary>
    public static MobilityScore NewMobilityScore(string code, string title)
    {
        return new MobilityScore(code, title, 0, 0, 0, MobilityScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityScoreStatus next)
    {
        switch (Status)
        {
            case MobilityScoreStatus.New:
                return next == MobilityScoreStatus.Done;
            case MobilityScoreStatus.Queued:
                switch (next)
                {
                    case MobilityScoreStatus.Done:
                    case MobilityScoreStatus.Failed:
                        return true;
                }
                return false;
            case MobilityScoreStatus.Running:
                return next == MobilityScoreStatus.Done || next == MobilityScoreStatus.Failed;
            case MobilityScoreStatus.Done:
                return next == MobilityScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MobilityScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version - (double)BatchSize, 2);
    }

    public MobilityScore Copy()
    {
        return new MobilityScore(Code, Title, Limit, Version, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
