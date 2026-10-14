namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg entry is in its life cycle.</summary>
public enum ReorgEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a reorg entry and its current status.</summary>
public sealed class ReorgEntry(string code, string name, string currency, double margin, string costCenter, string unit, Dictionary<string, string> attributes, string badge, int graceDays, double score, string managerId, ReorgEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public string ManagerId { get; set; } = managerId;
    public ReorgEntryStatus Status { get; set; } = status;

    /// <summary>Starts a reorg entry in status created.</summary>
    public static ReorgEntry NewReorgEntry(string code, string name)
    {
        return new ReorgEntry(
            code,
            name,
            "",
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            "",
            ReorgEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgEntryStatus next)
    {
        switch (Status)
        {
            case ReorgEntryStatus.Created:
                switch (next)
                {
                    case ReorgEntryStatus.Published:
                        return true;
                }
                return false;
            case ReorgEntryStatus.Validated:
                return next == ReorgEntryStatus.Published || next == ReorgEntryStatus.Retired;
            case ReorgEntryStatus.Published:
                return next == ReorgEntryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReorgEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"reorg entry"} {Code}";
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
        return Math.Round(Margin * Score, 2);
    }

    public ReorgEntry Copy()
    {
        return new ReorgEntry(
            Code,
            Name,
            Currency,
            Margin,
            CostCenter,
            Unit,
            Attributes,
            Badge,
            GraceDays,
            Score,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
