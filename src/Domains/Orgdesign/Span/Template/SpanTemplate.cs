namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span template is in its life cycle.</summary>
public enum SpanTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span template. Its code is unique within a tenant.</summary>
public sealed class SpanTemplate(string code, string label, string team, int retries, double score, double hours, double minimum, SpanTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public double Score { get; set; } = score;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public SpanTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a span template in status proposed.</summary>
    public static SpanTemplate NewSpanTemplate(string code, string label)
    {
        return new SpanTemplate(code, label, "", 0, 0.0, 0.0, 0.0, SpanTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanTemplateStatus next)
    {
        switch (Status)
        {
            case SpanTemplateStatus.Proposed:
                return next == SpanTemplateStatus.Rejected;
            case SpanTemplateStatus.Approved:
                return next == SpanTemplateStatus.Rejected || next == SpanTemplateStatus.Withdrawn;
            case SpanTemplateStatus.Rejected:
                return next == SpanTemplateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SpanTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span template"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries - Minimum, 2);
    }

    public SpanTemplate Copy()
    {
        return new SpanTemplate(Code, Label, Team, Retries, Score, Hours, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
