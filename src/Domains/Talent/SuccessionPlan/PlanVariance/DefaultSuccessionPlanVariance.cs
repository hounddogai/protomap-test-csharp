namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default succession plan variance is in its life cycle.</summary>
public enum DefaultSuccessionPlanVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One default succession plan variance, with what the planners need to act on it.</summary>
public sealed class DefaultSuccessionPlanVariance(string code, string title, string description, string timezone, double threshold, string label, DefaultSuccessionPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public double Threshold { get; set; } = threshold;
    public string Label { get; set; } = label;
    public DefaultSuccessionPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a default succession plan variance in status planned.</summary>
    public static DefaultSuccessionPlanVariance NewDefaultSuccessionPlanVariance(string code, string title)
    {
        return new DefaultSuccessionPlanVariance(
            code,
            title,
            "",
            "",
            0.0,
            "",
            DefaultSuccessionPlanVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSuccessionPlanVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSuccessionPlanVarianceStatus next)
    {
        switch (Status)
        {
            case DefaultSuccessionPlanVarianceStatus.Planned:
                switch (next)
                {
                    case DefaultSuccessionPlanVarianceStatus.Scheduled:
                    case DefaultSuccessionPlanVarianceStatus.Completed:
                        return true;
                }
                return false;
            case DefaultSuccessionPlanVarianceStatus.Scheduled:
                return next == DefaultSuccessionPlanVarianceStatus.Completed || next == DefaultSuccessionPlanVarianceStatus.Cancelled;
            case DefaultSuccessionPlanVarianceStatus.InProgress:
                return next == DefaultSuccessionPlanVarianceStatus.Completed || next == DefaultSuccessionPlanVarianceStatus.Cancelled;
            case DefaultSuccessionPlanVarianceStatus.Completed:
                switch (next)
                {
                    case DefaultSuccessionPlanVarianceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSuccessionPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default succession plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public DefaultSuccessionPlanVariance Copy()
    {
        return new DefaultSuccessionPlanVariance(Code, Title, Description, Timezone, Threshold, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
