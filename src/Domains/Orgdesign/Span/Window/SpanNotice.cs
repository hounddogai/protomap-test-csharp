namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span notice is in its life cycle.</summary>
public enum SpanNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a span notice and its current status.</summary>
public sealed class SpanNotice(string code, string title, int retries, string notes, string label, string employeeId, double maximum, SpanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Notes { get; set; } = notes;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Maximum { get; set; } = maximum;
    public SpanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a span notice in status created.</summary>
    public static SpanNotice NewSpanNotice(string code, string title)
    {
        return new SpanNotice(code, title, 0, "", "", "", 0.0, SpanNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanNoticeStatus next)
    {
        switch (Status)
        {
            case SpanNoticeStatus.Created:
                switch (next)
                {
                    case SpanNoticeStatus.Published:
                        return true;
                }
                return false;
            case SpanNoticeStatus.Validated:
                return next == SpanNoticeStatus.Published || next == SpanNoticeStatus.Retired;
            case SpanNoticeStatus.Published:
                switch (next)
                {
                    case SpanNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Retries - Maximum, 2);
    }

    public SpanNotice Copy()
    {
        return new SpanNotice(Code, Title, Retries, Notes, Label, EmployeeId, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
