namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default desk policy is in its life cycle.</summary>
public enum DefaultDeskPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default desk policy and its current status.</summary>
public sealed class DefaultDeskPolicy(string code, string title, string locale, string reviewer, int retries, DefaultDeskPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public DefaultDeskPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a default desk policy in status planned.</summary>
    public static DefaultDeskPolicy NewDefaultDeskPolicy(string code, string title)
    {
        return new DefaultDeskPolicy(code, title, "", "", 0, DefaultDeskPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultDeskPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultDeskPolicyStatus next)
    {
        switch (Status)
        {
            case DefaultDeskPolicyStatus.Planned:
                return next == DefaultDeskPolicyStatus.Completed;
            case DefaultDeskPolicyStatus.Scheduled:
                return next == DefaultDeskPolicyStatus.InProgress || next == DefaultDeskPolicyStatus.Cancelled;
            case DefaultDeskPolicyStatus.InProgress:
                switch (next)
                {
                    case DefaultDeskPolicyStatus.Completed:
                    case DefaultDeskPolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultDeskPolicyStatus.Completed:
                return next == DefaultDeskPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultDeskPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default desk policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default desk policy"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public DefaultDeskPolicy Copy()
    {
        return new DefaultDeskPolicy(Code, Title, Locale, Reviewer, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
