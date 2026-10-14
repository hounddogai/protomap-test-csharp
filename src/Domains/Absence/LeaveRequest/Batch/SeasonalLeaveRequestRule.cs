namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal leave request rule is in its life cycle.</summary>
public enum SeasonalLeaveRequestRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One seasonal leave request rule, with what the planners need to act on it.</summary>
public sealed class SeasonalLeaveRequestRule(string code, string label, string locale, List<string> tags, string timezone, int revision, SeasonalLeaveRequestRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public int Revision { get; set; } = revision;
    public SeasonalLeaveRequestRuleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal leave request rule in status planned.</summary>
    public static SeasonalLeaveRequestRule NewSeasonalLeaveRequestRule(string code, string label)
    {
        return new SeasonalLeaveRequestRule(
            code,
            label,
            "",
            new List<string>(),
            "",
            0,
            SeasonalLeaveRequestRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLeaveRequestRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLeaveRequestRuleStatus next)
    {
        switch (Status)
        {
            case SeasonalLeaveRequestRuleStatus.Planned:
                return next == SeasonalLeaveRequestRuleStatus.Scheduled;
            case SeasonalLeaveRequestRuleStatus.Scheduled:
                switch (next)
                {
                    case SeasonalLeaveRequestRuleStatus.InProgress:
                    case SeasonalLeaveRequestRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalLeaveRequestRuleStatus.InProgress:
                switch (next)
                {
                    case SeasonalLeaveRequestRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalLeaveRequestRuleStatus.Completed:
                return next == SeasonalLeaveRequestRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalLeaveRequestRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal leave request rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
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

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public SeasonalLeaveRequestRule Copy()
    {
        return new SeasonalLeaveRequestRule(Code, Label, Locale, Tags, Timezone, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
