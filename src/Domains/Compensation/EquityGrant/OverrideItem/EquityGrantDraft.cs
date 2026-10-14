namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant draft is in its life cycle.</summary>
public enum EquityGrantDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A equity grant draft as the equity grant team keeps it.</summary>
public sealed class EquityGrantDraft(string code, string title, double rate, string currency, string notes, double weight, string jobCode, int updatedAt, int createdAt, EquityGrantDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Currency { get; set; } = currency;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public int CreatedAt { get; set; } = createdAt;
    public EquityGrantDraftStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant draft in status new.</summary>
    public static EquityGrantDraft NewEquityGrantDraft(string code, string title)
    {
        return new EquityGrantDraft(code, title, 0.0, "", "", 0.0, "", 0, 0, EquityGrantDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantDraftStatus next)
    {
        switch (Status)
        {
            case EquityGrantDraftStatus.New:
                switch (next)
                {
                    case EquityGrantDraftStatus.Queued:
                    case EquityGrantDraftStatus.Done:
                        return true;
                }
                return false;
            case EquityGrantDraftStatus.Queued:
                return next == EquityGrantDraftStatus.Running || next == EquityGrantDraftStatus.Failed;
            case EquityGrantDraftStatus.Running:
                switch (next)
                {
                    case EquityGrantDraftStatus.Done:
                    case EquityGrantDraftStatus.Failed:
                        return true;
                }
                return false;
            case EquityGrantDraftStatus.Done:
                switch (next)
                {
                    case EquityGrantDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"equity grant draft"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Weight - (double)CreatedAt, 2);
    }

    public EquityGrantDraft Copy()
    {
        return new EquityGrantDraft(
            Code,
            Title,
            Rate,
            Currency,
            Notes,
            Weight,
            JobCode,
            UpdatedAt,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
