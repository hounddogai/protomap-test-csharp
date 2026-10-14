namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty batch is in its life cycle.</summary>
public enum WarrantyBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One warranty batch, with what the planners need to act on it.</summary>
public sealed class WarrantyBatch(string code, string name, double score, int priority, int retries, string description, int limit, string locale, int startsAt, double threshold, Dictionary<string, string> attributes, WarrantyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int Priority { get; set; } = priority;
    public int Retries { get; set; } = retries;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public string Locale { get; set; } = locale;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public WarrantyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a warranty batch in status proposed.</summary>
    public static WarrantyBatch NewWarrantyBatch(string code, string name)
    {
        return new WarrantyBatch(
            code,
            name,
            0.0,
            0,
            0,
            "",
            0,
            "",
            0,
            0.0,
            new Dictionary<string, string>(),
            WarrantyBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyBatchStatus next)
    {
        switch (Status)
        {
            case WarrantyBatchStatus.Proposed:
                switch (next)
                {
                    case WarrantyBatchStatus.Approved:
                    case WarrantyBatchStatus.Rejected:
                        return true;
                }
                return false;
            case WarrantyBatchStatus.Approved:
                return next == WarrantyBatchStatus.Rejected || next == WarrantyBatchStatus.Withdrawn;
            case WarrantyBatchStatus.Rejected:
                return next == WarrantyBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WarrantyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt * (double)Limit, 2);
    }

    public WarrantyBatch Copy()
    {
        return new WarrantyBatch(
            Code,
            Name,
            Score,
            Priority,
            Retries,
            Description,
            Limit,
            Locale,
            StartsAt,
            Threshold,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
