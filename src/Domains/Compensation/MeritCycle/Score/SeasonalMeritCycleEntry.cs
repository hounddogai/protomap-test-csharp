namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal merit cycle entry is in its life cycle.</summary>
public enum SeasonalMeritCycleEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal merit cycle entry. Its code is unique within a tenant.</summary>
public sealed class SeasonalMeritCycleEntry(string code, string label, int retries, List<string> tags, int attempts, SeasonalMeritCycleEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public SeasonalMeritCycleEntryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal merit cycle entry in status new.</summary>
    public static SeasonalMeritCycleEntry NewSeasonalMeritCycleEntry(string code, string label)
    {
        return new SeasonalMeritCycleEntry(code, label, 0, new List<string>(), 0, SeasonalMeritCycleEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMeritCycleEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMeritCycleEntryStatus next)
    {
        switch (Status)
        {
            case SeasonalMeritCycleEntryStatus.New:
                return next == SeasonalMeritCycleEntryStatus.Queued;
            case SeasonalMeritCycleEntryStatus.Queued:
                switch (next)
                {
                    case SeasonalMeritCycleEntryStatus.Running:
                        return true;
                }
                return false;
            case SeasonalMeritCycleEntryStatus.Running:
                switch (next)
                {
                    case SeasonalMeritCycleEntryStatus.Done:
                    case SeasonalMeritCycleEntryStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalMeritCycleEntryStatus.Done:
                return next == SeasonalMeritCycleEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalMeritCycleEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal merit cycle entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts * (double)Retries, 2);
    }

    public SeasonalMeritCycleEntry Copy()
    {
        return new SeasonalMeritCycleEntry(Code, Label, Retries, Tags, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
