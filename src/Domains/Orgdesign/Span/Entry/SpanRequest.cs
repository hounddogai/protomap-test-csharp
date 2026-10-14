namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span request is in its life cycle.</summary>
public enum SpanRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a span request and its current status.</summary>
public sealed class SpanRequest(string code, string name, int retries, int attempts, string category, string team, bool pinned, SpanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public SpanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a span request in status new.</summary>
    public static SpanRequest NewSpanRequest(string code, string name)
    {
        return new SpanRequest(code, name, 0, 0, "", "", false, SpanRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanRequestStatus next)
    {
        switch (Status)
        {
            case SpanRequestStatus.New:
                switch (next)
                {
                    case SpanRequestStatus.Running:
                        return true;
                }
                return false;
            case SpanRequestStatus.Queued:
                return next == SpanRequestStatus.Done || next == SpanRequestStatus.Failed;
            case SpanRequestStatus.Running:
                switch (next)
                {
                    case SpanRequestStatus.Done:
                    case SpanRequestStatus.Failed:
                        return true;
                }
                return false;
            case SpanRequestStatus.Done:
                return next == SpanRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SpanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Retries * (double)Attempts, 2);
    }

    public SpanRequest Copy()
    {
        return new SpanRequest(Code, Name, Retries, Attempts, Category, Team, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
