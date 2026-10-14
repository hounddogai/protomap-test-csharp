namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending warranty group is in its life cycle.</summary>
public enum PendingWarrantyGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending warranty group, with what the planners need to act on it.</summary>
public sealed class PendingWarrantyGroup(string code, string title, double ratio, string label, double amount, Dictionary<string, string> attributes, string ownerId, int retries, PendingWarrantyGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public int Retries { get; set; } = retries;
    public PendingWarrantyGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pending warranty group in status new.</summary>
    public static PendingWarrantyGroup NewPendingWarrantyGroup(string code, string title)
    {
        return new PendingWarrantyGroup(
            code,
            title,
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            PendingWarrantyGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingWarrantyGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingWarrantyGroupStatus next)
    {
        switch (Status)
        {
            case PendingWarrantyGroupStatus.New:
                return next == PendingWarrantyGroupStatus.Done;
            case PendingWarrantyGroupStatus.Queued:
                return next == PendingWarrantyGroupStatus.Running || next == PendingWarrantyGroupStatus.Failed;
            case PendingWarrantyGroupStatus.Running:
                switch (next)
                {
                    case PendingWarrantyGroupStatus.Done:
                        return true;
                }
                return false;
            case PendingWarrantyGroupStatus.Done:
                return next == PendingWarrantyGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingWarrantyGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending warranty group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending warranty group"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)Retries + Ratio, 2);
    }

    public PendingWarrantyGroup Copy()
    {
        return new PendingWarrantyGroup(Code, Title, Ratio, Label, Amount, Attributes, OwnerId, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
