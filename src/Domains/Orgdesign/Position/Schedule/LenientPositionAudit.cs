namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient position audit is in its life cycle.</summary>
public enum LenientPositionAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient position audit. Its code is unique within a tenant.</summary>
public sealed class LenientPositionAudit(string code, string label, int durationMinutes, double amount, Dictionary<string, string> attributes, int revision, bool visible, LenientPositionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public bool Visible { get; set; } = visible;
    public LenientPositionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a lenient position audit in status open.</summary>
    public static LenientPositionAudit NewLenientPositionAudit(string code, string label)
    {
        return new LenientPositionAudit(
            code,
            label,
            0,
            0.0,
            new Dictionary<string, string>(),
            0,
            false,
            LenientPositionAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPositionAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPositionAuditStatus next)
    {
        switch (Status)
        {
            case LenientPositionAuditStatus.Open:
                switch (next)
                {
                    case LenientPositionAuditStatus.InReview:
                    case LenientPositionAuditStatus.Archived:
                        return true;
                }
                return false;
            case LenientPositionAuditStatus.InReview:
                return next == LenientPositionAuditStatus.Reopened;
            case LenientPositionAuditStatus.Resolved:
                switch (next)
                {
                    case LenientPositionAuditStatus.Reopened:
                    case LenientPositionAuditStatus.Archived:
                        return true;
                }
                return false;
            case LenientPositionAuditStatus.Reopened:
                switch (next)
                {
                    case LenientPositionAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPositionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient position audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"lenient position audit"} {Code}";
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

    public double AdjustedLoad()
    {
        return Math.Round(Amount - (double)Revision, 2);
    }

    public LenientPositionAudit Copy()
    {
        return new LenientPositionAudit(Code, Label, DurationMinutes, Amount, Attributes, Revision, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
