namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool estimate is in its life cycle.</summary>
public enum AdaptiveBonusPoolEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive bonus pool estimate. Its code is unique within a tenant.</summary>
public sealed class AdaptiveBonusPoolEstimate(string code, string name, string phone, int sequence, string notes, string title, int retries, string badge, int quantity, Dictionary<string, string> attributes, double rate, AdaptiveBonusPoolEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public AdaptiveBonusPoolEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool estimate in status proposed.</summary>
    public static AdaptiveBonusPoolEstimate NewAdaptiveBonusPoolEstimate(string code, string name)
    {
        return new AdaptiveBonusPoolEstimate(
            code,
            name,
            "",
            0,
            "",
            "",
            0,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            AdaptiveBonusPoolEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolEstimateStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolEstimateStatus.Proposed:
                switch (next)
                {
                    case AdaptiveBonusPoolEstimateStatus.Rejected:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolEstimateStatus.Approved:
                return next == AdaptiveBonusPoolEstimateStatus.Withdrawn;
            case AdaptiveBonusPoolEstimateStatus.Rejected:
                return next == AdaptiveBonusPoolEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive bonus pool estimate"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round(Rate + (double)Quantity, 2);
    }

    public AdaptiveBonusPoolEstimate Copy()
    {
        return new AdaptiveBonusPoolEstimate(
            Code,
            Name,
            Phone,
            Sequence,
            Notes,
            Title,
            Retries,
            Badge,
            Quantity,
            Attributes,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
