namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal headcount window is in its life cycle.</summary>
public enum SeasonalHeadcountWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal headcount window. Its code is unique within a tenant.</summary>
public sealed class SeasonalHeadcountWindow(string code, string label, int windowDays, int retries, string jobCode, string skill, string description, int sequence, string region, double margin, string badge, SeasonalHeadcountWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public string Skill { get; set; } = skill;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public SeasonalHeadcountWindowStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal headcount window in status idle.</summary>
    public static SeasonalHeadcountWindow NewSeasonalHeadcountWindow(string code, string label)
    {
        return new SeasonalHeadcountWindow(
            code,
            label,
            0,
            0,
            "",
            "",
            "",
            0,
            "",
            0.0,
            "",
            SeasonalHeadcountWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHeadcountWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHeadcountWindowStatus next)
    {
        switch (Status)
        {
            case SeasonalHeadcountWindowStatus.Idle:
                return next == SeasonalHeadcountWindowStatus.Warming || next == SeasonalHeadcountWindowStatus.Stopped;
            case SeasonalHeadcountWindowStatus.Warming:
                switch (next)
                {
                    case SeasonalHeadcountWindowStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalHeadcountWindowStatus.Ready:
                return next == SeasonalHeadcountWindowStatus.Draining;
            case SeasonalHeadcountWindowStatus.Draining:
                return next == SeasonalHeadcountWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalHeadcountWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal headcount window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin * (double)Retries, 2);
    }

    public SeasonalHeadcountWindow Copy()
    {
        return new SeasonalHeadcountWindow(
            Code,
            Label,
            WindowDays,
            Retries,
            JobCode,
            Skill,
            Description,
            Sequence,
            Region,
            Margin,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
