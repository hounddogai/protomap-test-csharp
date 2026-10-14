namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient engagement template is in its life cycle.</summary>
public enum LenientEngagementTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient engagement template as the engagement team keeps it.</summary>
public sealed class LenientEngagementTemplate(string code, string label, string badge, int graceDays, int expiresAt, string timezone, LenientEngagementTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public LenientEngagementTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient engagement template in status new.</summary>
    public static LenientEngagementTemplate NewLenientEngagementTemplate(string code, string label)
    {
        return new LenientEngagementTemplate(code, label, "", 0, 0, "", LenientEngagementTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEngagementTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEngagementTemplateStatus next)
    {
        switch (Status)
        {
            case LenientEngagementTemplateStatus.New:
                switch (next)
                {
                    case LenientEngagementTemplateStatus.Queued:
                        return true;
                }
                return false;
            case LenientEngagementTemplateStatus.Queued:
                return next == LenientEngagementTemplateStatus.Failed;
            case LenientEngagementTemplateStatus.Running:
                return next == LenientEngagementTemplateStatus.Done;
            case LenientEngagementTemplateStatus.Done:
                return next == LenientEngagementTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientEngagementTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient engagement template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient engagement template"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)GraceDays * (double)ExpiresAt, 2);
    }

    public LenientEngagementTemplate Copy()
    {
        return new LenientEngagementTemplate(Code, Label, Badge, GraceDays, ExpiresAt, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
