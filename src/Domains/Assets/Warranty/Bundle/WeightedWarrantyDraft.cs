namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted warranty draft is in its life cycle.</summary>
public enum WeightedWarrantyDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted warranty draft as the warranty team keeps it.</summary>
public sealed class WeightedWarrantyDraft(string code, string title, string description, string summary, int windowDays, string badge, int retries, WeightedWarrantyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public string Badge { get; set; } = badge;
    public int Retries { get; set; } = retries;
    public WeightedWarrantyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a weighted warranty draft in status new.</summary>
    public static WeightedWarrantyDraft NewWeightedWarrantyDraft(string code, string title)
    {
        return new WeightedWarrantyDraft(code, title, "", "", 0, "", 0, WeightedWarrantyDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedWarrantyDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedWarrantyDraftStatus next)
    {
        switch (Status)
        {
            case WeightedWarrantyDraftStatus.New:
                switch (next)
                {
                    case WeightedWarrantyDraftStatus.Failed:
                        return true;
                }
                return false;
            case WeightedWarrantyDraftStatus.Queued:
                switch (next)
                {
                    case WeightedWarrantyDraftStatus.Running:
                    case WeightedWarrantyDraftStatus.Done:
                        return true;
                }
                return false;
            case WeightedWarrantyDraftStatus.Running:
                switch (next)
                {
                    case WeightedWarrantyDraftStatus.Done:
                    case WeightedWarrantyDraftStatus.Failed:
                        return true;
                }
                return false;
            case WeightedWarrantyDraftStatus.Done:
                switch (next)
                {
                    case WeightedWarrantyDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedWarrantyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted warranty draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted warranty draft"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Retries * (double)WindowDays, 2);
    }

    public WeightedWarrantyDraft Copy()
    {
        return new WeightedWarrantyDraft(Code, Title, Description, Summary, WindowDays, Badge, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
