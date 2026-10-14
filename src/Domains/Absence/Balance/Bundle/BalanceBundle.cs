namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance bundle is in its life cycle.</summary>
public enum BalanceBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A balance bundle. Its code is unique within a tenant.</summary>
public sealed class BalanceBundle(string code, string label, int updatedAt, string department, string channel, string reviewer, int endsAt, Dictionary<string, string> attributes, double ratio, BalanceBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public BalanceBundleStatus Status { get; set; } = status;

    /// <summary>Starts a balance bundle in status planned.</summary>
    public static BalanceBundle NewBalanceBundle(string code, string label)
    {
        return new BalanceBundle(
            code,
            label,
            0,
            "",
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            BalanceBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceBundleStatus next)
    {
        switch (Status)
        {
            case BalanceBundleStatus.Planned:
                switch (next)
                {
                    case BalanceBundleStatus.Scheduled:
                    case BalanceBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceBundleStatus.Scheduled:
                switch (next)
                {
                    case BalanceBundleStatus.InProgress:
                    case BalanceBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceBundleStatus.InProgress:
                return next == BalanceBundleStatus.Completed;
            case BalanceBundleStatus.Completed:
                return next == BalanceBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BalanceBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"balance bundle"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt * Ratio, 2);
    }

    public BalanceBundle Copy()
    {
        return new BalanceBundle(
            Code,
            Label,
            UpdatedAt,
            Department,
            Channel,
            Reviewer,
            EndsAt,
            Attributes,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
