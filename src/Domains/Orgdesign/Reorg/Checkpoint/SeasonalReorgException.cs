namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal reorg exception is in its life cycle.</summary>
public enum SeasonalReorgExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal reorg exception as the reorg team keeps it.</summary>
public sealed class SeasonalReorgException(string code, string label, double ceiling, string currency, int leaveDays, double budget, string checksum, int attempts, string name, SeasonalReorgExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Currency { get; set; } = currency;
    public int LeaveDays { get; set; } = leaveDays;
    public double Budget { get; set; } = budget;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public string Name { get; set; } = name;
    public SeasonalReorgExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal reorg exception in status new.</summary>
    public static SeasonalReorgException NewSeasonalReorgException(string code, string label)
    {
        return new SeasonalReorgException(code, label, 0.0, "", 0, 0.0, "", 0, "", SeasonalReorgExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalReorgExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalReorgExceptionStatus next)
    {
        switch (Status)
        {
            case SeasonalReorgExceptionStatus.New:
                return next == SeasonalReorgExceptionStatus.Queued;
            case SeasonalReorgExceptionStatus.Queued:
                switch (next)
                {
                    case SeasonalReorgExceptionStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalReorgExceptionStatus.Running:
                return next == SeasonalReorgExceptionStatus.Failed;
            case SeasonalReorgExceptionStatus.Done:
                switch (next)
                {
                    case SeasonalReorgExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalReorgExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal reorg exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal reorg exception"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts * Ceiling, 2);
    }

    public SeasonalReorgException Copy()
    {
        return new SeasonalReorgException(
            Code,
            Label,
            Ceiling,
            Currency,
            LeaveDays,
            Budget,
            Checksum,
            Attempts,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
