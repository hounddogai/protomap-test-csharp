namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band milestone is in its life cycle.</summary>
public enum PayBandMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pay band milestone as the pay band team keeps it.</summary>
public sealed class PayBandMilestone(string code, string title, double margin, int revision, Dictionary<string, string> attributes, PayBandMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PayBandMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pay band milestone in status idle.</summary>
    public static PayBandMilestone NewPayBandMilestone(string code, string title)
    {
        return new PayBandMilestone(
            code,
            title,
            0.0,
            0,
            new Dictionary<string, string>(),
            PayBandMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandMilestoneStatus next)
    {
        switch (Status)
        {
            case PayBandMilestoneStatus.Idle:
                switch (next)
                {
                    case PayBandMilestoneStatus.Warming:
                    case PayBandMilestoneStatus.Ready:
                        return true;
                }
                return false;
            case PayBandMilestoneStatus.Warming:
                return next == PayBandMilestoneStatus.Ready;
            case PayBandMilestoneStatus.Ready:
                switch (next)
                {
                    case PayBandMilestoneStatus.Draining:
                    case PayBandMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case PayBandMilestoneStatus.Draining:
                switch (next)
                {
                    case PayBandMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double EffectiveVolume()
    {
        return Math.Round(Margin - (double)Revision, 2);
    }

    public PayBandMilestone Copy()
    {
        return new PayBandMilestone(Code, Title, Margin, Revision, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
