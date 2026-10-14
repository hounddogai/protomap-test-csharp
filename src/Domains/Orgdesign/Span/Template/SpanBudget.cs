namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span budget is in its life cycle.</summary>
public enum SpanBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a span budget and its current status.</summary>
public sealed class SpanBudget(string code, string name, string badge, bool pinned, int revision, string channel, string segment, string checksum, int updatedAt, int batchSize, int retries, SpanBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public bool Pinned { get; set; } = pinned;
    public int Revision { get; set; } = revision;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public SpanBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a span budget in status new.</summary>
    public static SpanBudget NewSpanBudget(string code, string name)
    {
        return new SpanBudget(code, name, "", false, 0, "", "", "", 0, 0, 0, SpanBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanBudgetStatus next)
    {
        switch (Status)
        {
            case SpanBudgetStatus.New:
                return next == SpanBudgetStatus.Running || next == SpanBudgetStatus.Failed;
            case SpanBudgetStatus.Queued:
                return next == SpanBudgetStatus.Failed;
            case SpanBudgetStatus.Running:
                switch (next)
                {
                    case SpanBudgetStatus.Done:
                    case SpanBudgetStatus.Failed:
                        return true;
                }
                return false;
            case SpanBudgetStatus.Done:
                return next == SpanBudgetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SpanBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)BatchSize - (double)Revision, 2);
    }

    public SpanBudget Copy()
    {
        return new SpanBudget(
            Code,
            Name,
            Badge,
            Pinned,
            Revision,
            Channel,
            Segment,
            Checksum,
            UpdatedAt,
            BatchSize,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
