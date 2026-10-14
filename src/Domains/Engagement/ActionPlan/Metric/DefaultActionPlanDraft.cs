namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan draft is in its life cycle.</summary>
public enum DefaultActionPlanDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a default action plan draft and its current status.</summary>
public sealed class DefaultActionPlanDraft(string code, string label, string channel, double weight, int startsAt, double budget, DefaultActionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public double Budget { get; set; } = budget;
    public DefaultActionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan draft in status new.</summary>
    public static DefaultActionPlanDraft NewDefaultActionPlanDraft(string code, string label)
    {
        return new DefaultActionPlanDraft(code, label, "", 0.0, 0, 0.0, DefaultActionPlanDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanDraftStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanDraftStatus.New:
                switch (next)
                {
                    case DefaultActionPlanDraftStatus.Running:
                        return true;
                }
                return false;
            case DefaultActionPlanDraftStatus.Queued:
                switch (next)
                {
                    case DefaultActionPlanDraftStatus.Failed:
                        return true;
                }
                return false;
            case DefaultActionPlanDraftStatus.Running:
                return next == DefaultActionPlanDraftStatus.Failed;
            case DefaultActionPlanDraftStatus.Done:
                switch (next)
                {
                    case DefaultActionPlanDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt + Weight, 2);
    }

    public DefaultActionPlanDraft Copy()
    {
        return new DefaultActionPlanDraft(Code, Label, Channel, Weight, StartsAt, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
